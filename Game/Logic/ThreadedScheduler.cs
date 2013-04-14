#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Data;
using Game.Database;
using Game.Setup;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Logic
{
    public class ThreadedScheduler : IScheduler
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly ScheduleComparer comparer = new ScheduleComparer();

        private readonly Dictionary<int, ManualResetEvent> doneEvents = new Dictionary<int, ManualResetEvent>();

        private readonly List<ISchedule> schedules = new List<ISchedule>();

        private readonly object schedulesLock = new object();

        private readonly Timer timer;

        private int actionsFired;

        private DateTime lastProbe;

        private int lastScheduleSize;

        private DateTime nextFire;

        private int nextId;

        private readonly TaskFactory factory;

        [ThreadStatic]
        private static ISchedule actionExecuting;

        public ISchedule ActionExecuting {
            get
            {
                return actionExecuting;
            }
        }

        public ThreadedScheduler()
        {
            timer = new Timer(DispatchAction,
                              // main timer
                              null,
                              Timeout.Infinite,
                              Timeout.Infinite);
            
            factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(Config.scheduler_threads));
        }

        public bool Paused { get; private set; }

        public void Probe(out DateTime outLastProbe, out int outActionsFired, out int schedulerSize, out int schedulerDelta, out DateTime outNextFire)
        {
            lock (schedulesLock)
            {
                outLastProbe = lastProbe;
                outActionsFired = actionsFired;
                schedulerSize = schedules.Count;
                schedulerDelta = schedulerSize - lastScheduleSize;
                outNextFire = nextFire;

                lastScheduleSize = schedulerSize;
                lastProbe = SystemClock.Now;
                actionsFired = 0;
            }
        }

        public void Pause()
        {
            ManualResetEvent[] events;

            lock (schedulesLock)
            {
                Paused = true;
                logger.Info("Scheduler paused");
                SetTimer(Timeout.Infinite);
                events = new ManualResetEvent[doneEvents.Count];
                doneEvents.Values.CopyTo(events, 0);
            }

            foreach (var evt in events)
            {
                evt.WaitOne();
            }
        }

        public void Resume()
        {
            lock (schedulesLock)
            {
                Paused = false;
                logger.Info("Scheduler resumed");
                SetNextActionTime();
            }
        }

        public void Put(ISchedule schedule)
        {
            lock (schedulesLock)
            {
                if (schedule.IsScheduled)
                {
                    throw new Exception("Attempting to schedule action that is already scheduled");
                }

                int index = schedules.BinarySearch(schedule, comparer);

                if (index < 0)
                {
                    index = ~index;
                    schedules.Insert(index, schedule);
                }
                else
                {
                    schedules.Insert(index, schedule);
                }

                schedule.IsScheduled = true;

                if (logger.IsTraceEnabled)
                {
                    logger.Trace(String.Format("Schedule added index[{0}] total[{1}].", index, schedules.Count));
                }

                if (index == 0)
                {
                    SetNextActionTime();
                }
            }
        }

        public bool Remove(ISchedule schedule)
        {
            lock (schedulesLock)
            {
                if (!schedule.IsScheduled)
                {
                    return false;
                }

                if (schedules.Remove(schedule))
                {
                    schedule.IsScheduled = false;
                    SetNextActionTime();
                    return true;
                }

                logger.Warn("Action was said to be scheduled but was not found in scheduler during a remove");

                return false;
            }
        }

        private void SetTimer(long ms)
        {
            if (ms == Timeout.Infinite)
            {
                logger.Debug(String.Format("Timer sleeping"));
                nextFire = DateTime.MinValue;
            }
            else
            {
                /*if (logger.IsDebugEnabled && ms > 0)
                {
                    logger.Debug(String.Format("Next schedule in {0} milliseconds.", ms));
                }*/

                nextFire = SystemClock.Now.AddMilliseconds(ms);
            }

            timer.Change(ms, Timeout.Infinite);
        }

        // call back for the timer function
        private void DispatchAction(object obj) // obj ignored
        {
            lock (schedulesLock)
            {
                if (schedules.Count == 0 || Paused)
                {
                    logger.Debug("In DispatchAction but no schedules");
                    return;
                }

                actionsFired++;

                // Get the schedule that is supposed to fire
                ISchedule next = schedules[0];
                schedules.RemoveAt(0);
                next.IsScheduled = false;

                // Convert it into a job that has a semaphore we can wait on
                // if we need to wait for it to finish
                var job = new ScheduledJob {Id = nextId, ResetEvent = new ManualResetEvent(false), Schedule = next};

                // Wrap around if the id is getting large
                if (nextId == Int32.MaxValue - 1)
                {
                    nextId = 0;
                }
                else
                {
                    ++nextId;
                }

                doneEvents.Add(job.Id, job.ResetEvent);

                factory.StartNew(() => ExecuteAction(job));                

                SetNextActionTime();
            }
        }

        private void ExecuteAction(object obj)
        {
            var job = (ScheduledJob)obj;

            actionExecuting = job.Schedule;

            job.Schedule.Callback(null);

            actionExecuting = null;

            lock (schedulesLock)
            {
                doneEvents.Remove(job.Id);
                job.ResetEvent.Set();
            }
        }

        /// <summary>
        /// Set the time when the timer should wake up to invoke the next schedule
        /// </summary>
        private void SetNextActionTime()
        {
            if (Paused)
            {
                return;
            }

            if (schedules.Count == 0)
            {
                logger.Debug("No actions available.");
                SetTimer(Timeout.Infinite);
                return;
            }

            TimeSpan ts = schedules[0].Time.Subtract(SystemClock.Now);
            long ms = Math.Max(0, (long)ts.TotalMilliseconds);
            SetTimer(ms);
        }

        private class ScheduledJob
        {
            public ManualResetEvent ResetEvent { get; set; }

            public ISchedule Schedule { get; set; }

            public int Id { get; set; }

            public DateTime Created { get; private set; }

            public ScheduledJob()
            {
                Created = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Provides a task scheduler that ensures a maximum concurrency level while
        /// running on top of the ThreadPool.
        /// </summary>
        public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
        {
            /// <summary>Whether the current thread is processing work items.</summary>
            [ThreadStatic]
            private static bool _currentThreadIsProcessingItems;

            /// <summary>The list of tasks to be executed.</summary>
            private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

            /// <summary>The maximum concurrency level allowed by this scheduler.</summary>
            private readonly int _maxDegreeOfParallelism;

            /// <summary>Whether the scheduler is currently processing work items.</summary>
            private int _delegatesQueuedOrRunning = 0; // protected by lock(_tasks)

            /// <summary>
            /// Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the
            /// specified degree of parallelism.
            /// </summary>
            /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism provided by this scheduler.</param>
            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
            {
                if (maxDegreeOfParallelism < 1)
                    throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
                _maxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            /// <summary>Queues a task to the scheduler.</summary>
            /// <param name="task">The task to be queued.</param>
            protected override sealed void QueueTask(Task task)
            {
                // Add the task to the list of tasks to be processed.  If there aren't enough
                // delegates currently queued or running to process tasks, schedule another.
                lock (_tasks)
                {
                    _tasks.AddLast(task);
                    if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                    {
                        ++_delegatesQueuedOrRunning;
                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }

            /// <summary>
            /// Informs the ThreadPool that there's work to be executed for this scheduler.
            /// </summary>
            private void NotifyThreadPoolOfPendingWork()
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                    {
                        // Note that the current thread is now processing work items.
                        // This is necessary to enable inlining of tasks into this thread.
                        _currentThreadIsProcessingItems = true;
                        try
                        {
                            // Process all available items in the queue.
                            while (true)
                            {
                                Task item;
                                lock (_tasks)
                                {
                                    // When there are no more items to be processed,
                                    // note that we're done processing, and get out.
                                    if (_tasks.Count == 0)
                                    {
                                        --_delegatesQueuedOrRunning;
                                        break;
                                    }

                                    // Get the next item from the queue
                                    item = _tasks.First.Value;
                                    _tasks.RemoveFirst();
                                }

                                // Execute the task we pulled out of the queue
                                base.TryExecuteTask(item);
                            }
                        }
                                // We're done processing items on the current thread
                        finally
                        {
                            _currentThreadIsProcessingItems = false;
                        }
                    },
                                                   null);
            }

            /// <summary>Attempts to execute the specified task on the current thread.</summary>
            /// <param name="task">The task to be executed.</param>
            /// <param name="taskWasPreviouslyQueued"></param>
            /// <returns>Whether the task could be executed on the current thread.</returns>
            protected override sealed bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                // If this thread isn't already processing a task, we don't support inlining
                if (!_currentThreadIsProcessingItems)
                    return false;

                // If the task was previously queued, remove it from the queue
                if (taskWasPreviouslyQueued)
                    TryDequeue(task);

                // Try to run the task.
                return base.TryExecuteTask(task);
            }

            /// <summary>Attempts to remove a previously scheduled task from the scheduler.</summary>
            /// <param name="task">The task to be removed.</param>
            /// <returns>Whether the task could be found and removed.</returns>
            protected override sealed bool TryDequeue(Task task)
            {
                lock (_tasks)
                    return _tasks.Remove(task);
            }

            /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
            public override sealed int MaximumConcurrencyLevel
            {
                get
                {
                    return _maxDegreeOfParallelism;
                }
            }

            /// <summary>Gets an enumerable of the tasks currently scheduled on this scheduler.</summary>
            /// <returns>An enumerable of the tasks currently scheduled.</returns>
            protected override sealed IEnumerable<Task> GetScheduledTasks()
            {
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(_tasks, ref lockTaken);
                    if (lockTaken)
                        return _tasks.ToArray();
                    else
                        throw new NotSupportedException();
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(_tasks);
                }
            }
        }
    }
}