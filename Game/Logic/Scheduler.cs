#region

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Game.Logic {
    
    class ScheduledJob {
        public ManualResetEvent ResetEvent { get; set; }
        public ISchedule Schedule { get; set; }
        public int Id { get; set; }
    }

    public class Scheduler {
        private int nextId;
        private readonly object schedulesLock = new object();
        private readonly ScheduleComparer comparer = new ScheduleComparer();
        private readonly List<ISchedule> schedules = new List<ISchedule>();
        private readonly Timer timer;
        private readonly Dictionary<int, ManualResetEvent> doneEvents = new Dictionary<int, ManualResetEvent>();

        public bool Paused { get; private set; }

        public Scheduler() {
            timer = new Timer(DispatchAction, // main timer
                              null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Pause() {
            ManualResetEvent[] events;

            lock (schedulesLock) {
                Paused = true;
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                events = new ManualResetEvent[doneEvents.Count];
                doneEvents.Values.CopyTo(events, 0);
            }

            foreach (ManualResetEvent evt in events) {
                evt.WaitOne();
            }
        }

        public void Resume() {
            lock (schedulesLock) {
                Paused = false;
                SetNextActionTime();
            }
        }

        public void Put(ISchedule schedule) {
            lock (schedulesLock) {
                int index = schedules.BinarySearch(schedule, comparer);

                if (index < 0) {
                    index = ~index;
                    schedules.Insert(index, schedule);
                }
                else
                    schedules.Insert(index, schedule);

                if (index == 0)
                    SetNextActionTime();
            }
        }

        public void Del(ISchedule schedule) {
            lock (schedulesLock) {
                if (schedules.Remove(schedule))
                    SetNextActionTime();
            }
        }

        // call back for the timer function
        private void DispatchAction(object obj) // obj ignored
        {            
            lock (schedulesLock) {
                if (schedules.Count == 0 || Paused)
                    return;

                // Get the schedule that is supposed to fire
                ISchedule next = schedules[0];
                schedules.RemoveAt(0);

                // Convert it into a job that has a semaphore we can wait on
                // if we need to wait for it to finish
                ScheduledJob job = new ScheduledJob {
                    Id = nextId,
                    ResetEvent = new ManualResetEvent(false),
                    Schedule = next
                };

                // Wrap around if the id is getting large
                if (nextId == int.MaxValue - 1)
                    nextId = 0;
                else
                    ++nextId;

                doneEvents.Add(job.Id, job.ResetEvent);
                ThreadPool.QueueUserWorkItem(DispatchThread, job);

                SetNextActionTime();
            }
        }

        private void DispatchThread(object obj) {

            ScheduledJob job = (ScheduledJob)obj;

            job.Schedule.Callback(null);
            
            lock (schedulesLock) {
                doneEvents.Remove(job.Id);
                job.ResetEvent.Set();
            }
        }

        // method to set the time when the timer should wake up to invoke the next schedule
        private void SetNextActionTime() {
            if (Paused)
                return;

            if (schedules.Count == 0) {
                timer.Change(Timeout.Infinite, Timeout.Infinite); // this will put the timer to sleep
                return;
            }

            TimeSpan ts = schedules[0].Time.Subtract(DateTime.UtcNow);
            if (ts < TimeSpan.Zero)
                ts = TimeSpan.Zero; // cannot be negative !
            timer.Change((int)ts.TotalMilliseconds, Timeout.Infinite); // invoke after the timespan
            //Global.Logger.Info("Next schedule in " + ts.TotalSeconds + " seconds.");
        }
    }
}