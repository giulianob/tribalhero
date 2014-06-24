#region

using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using Game.Util;

#endregion

namespace Game.Logic
{
    public class ThreadedScheduler : IScheduler
    {
        private readonly ITaskScheduler taskScheduler;

        private static readonly ILogger Logger = LoggerFactory.Current.GetLogger<ThreadedScheduler>();

        private readonly ScheduleComparer comparer = new ScheduleComparer();

        private readonly Dictionary<int, ManualResetEvent> doneEvents = new Dictionary<int, ManualResetEvent>();

        private readonly List<ISchedule> schedules = new List<ISchedule>();

        private readonly object schedulesLock = new object();

        private readonly Timer timer;

        private int actionsFired;

        private int actionTotalMilliseconds;

        private DateTime lastProbe;

        private int lastScheduleSize;

        private DateTime nextFire;

        private int nextId;

        [ThreadStatic]
        private static ISchedule actionExecuting;

        public ISchedule ActionExecuting {
            get
            {
                return actionExecuting;
            }
        }

        public ThreadedScheduler(ITaskScheduler taskScheduler)
        {
            this.taskScheduler = taskScheduler;

            timer = new Timer(DispatchAction,
                              // main timer
                              null,
                              Timeout.Infinite,
                              Timeout.Infinite);
        }

        public bool Paused { get; private set; }

        public void Probe(out DateTime outLastProbe, 
            out int outActionsFired, 
            out int schedulerSize, 
            out int schedulerDelta, 
            out DateTime outNextFire,
            out int outActionTotalMilliseconds)
        {
            lock (schedulesLock)
            {
                outLastProbe = lastProbe;
                outActionsFired = actionsFired;
                schedulerSize = schedules.Count;
                schedulerDelta = schedulerSize - lastScheduleSize;
                outNextFire = nextFire;
                outActionTotalMilliseconds = actionTotalMilliseconds;

                lastScheduleSize = schedulerSize;
                lastProbe = SystemClock.Now;
                actionTotalMilliseconds = 0;
                actionsFired = 0;
            }
        }

        public void Pause()
        {
            ManualResetEvent[] events;

            lock (schedulesLock)
            {
                Paused = true;
                Logger.Info("Scheduler paused");
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
                Logger.Info("Scheduler resumed");
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

                //if (Logger.IsDebugEnabled)
                //{
                //    Log(schedule, String.Format("Schedule added index[{0}] total[{1}].", index, schedules.Count));
                //}

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

                Logger.Warn("Action was said to be scheduled but was not found in scheduler during a remove");

                return false;
            }
        }

        private void SetTimer(long ms)
        {
            if (ms == Timeout.Infinite)
            {
                Logger.Debug(String.Format("Scheduler sleeping"));
                nextFire = DateTime.MinValue;
            }
            else
            {
//                if (Logger.IsDebugEnabled && ms > 0)
//                {
//                    Logger.Debug(String.Format("Next schedule in {0} ms.", ms));
//                }

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
                    Logger.Debug("In DispatchAction but no schedules");
                    return;
                }
                
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

                //Log(job.Schedule, "Queueing for execution");

                taskScheduler.QueueWorkItem(() => ExecuteAction(job));

                SetNextActionTime();
            }
        }

        private void ExecuteAction(object obj)
        {           
            var job = (ScheduledJob)obj;

            //Log(job.Schedule, "Executing action");

            actionExecuting = job.Schedule;

            var scheduledDelta = SystemClock.Now.Subtract(job.Schedule.Time);
            if (scheduledDelta.TotalSeconds > 1)
            {
                Log(job.Schedule, string.Format("Action fired {0}ms late", scheduledDelta));
            }
            else if (scheduledDelta.TotalSeconds < -1)
            {
                Log(job.Schedule, string.Format("Action fired {0}ms early", scheduledDelta));
            }
            

            var startTicks = Environment.TickCount;


            job.Schedule.Callback(null);


            var deltaTicks = Environment.TickCount - startTicks;
            if (deltaTicks > 1000)
            {
                Log(job.Schedule, string.Format("Slow action took {0}ms to complete.", deltaTicks));
            }

            Interlocked.Increment(ref actionsFired);
            Interlocked.Add(ref actionTotalMilliseconds, deltaTicks);

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
                Logger.Debug("No actions available.");
                SetTimer(Timeout.Infinite);
                return;
            }

            TimeSpan ts = schedules[0].Time.Subtract(SystemClock.Now);
            long ms = Math.Max(0, (long)ts.TotalMilliseconds);
            SetTimer(ms);
        }

        private void Log(ISchedule schedule, string messagePrefix)
        {
            var gameAction = schedule as GameAction;
            if (gameAction == null)
            {                
                Logger.Warn("{0} ScheduleType[{1}] ScheduleTime[{2}]",
                            messagePrefix,
                            schedule.GetType().FullName,
                            schedule.Time);
            }
            else
            {
                Logger.Warn("{0}. ScheduleType[{1}] ScheduleTime[{2}] LocationType[{3}] LocationId[{4}] ActionId[{5}]",
                            messagePrefix,
                            schedule.GetType().FullName,
                            schedule.Time,
                            gameAction.Location.LocationType.ToString(),
                            gameAction.Location.LocationId,
                            gameAction.ActionId);
            }
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
    }
}