#region

using System;
using System.Collections.Generic;
using System.Threading;
using Game.Data;
using Game.Util;

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

        private int lastScheduleSize;
        private int actionsFired;
        private DateTime lastProbe;
        private DateTime nextFire;

        public bool Paused { get; private set; }

        public Scheduler() {
            timer = new Timer(DispatchAction, // main timer
                              null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Probe(out DateTime outLastProbe, out int outActionsFired, out int schedulerSize, out int schedulerDelta, out DateTime outNextFire) {
            lock (schedulesLock) {

                outLastProbe = lastProbe;
                outActionsFired = actionsFired;
                schedulerSize = schedules.Count;
                schedulerDelta = schedulerSize - lastScheduleSize;
                outNextFire = nextFire;
                
                lastScheduleSize = schedulerSize;
                lastProbe = DateTime.UtcNow;
                actionsFired = 0;
            }
        }

        private void SetTimer(int ms) {
            if (ms == Timeout.Infinite)
            {
                Global.Logger.Debug(string.Format("Timer sleeping"));
                nextFire = DateTime.MinValue;
            }
            else {
                Global.Logger.Debug(string.Format("Next schedule in {0} milliseconds.", ms));
                nextFire = DateTime.UtcNow.AddMilliseconds(ms);
            }

            timer.Change(ms, Timeout.Infinite);
        }


        public void Pause() {
            ManualResetEvent[] events;

            lock (schedulesLock) {
                Paused = true;
                Global.Logger.Debug("Scheduler paused.");
                SetTimer(Timeout.Infinite);
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

                if (schedule.IsScheduled) throw new Exception("Attempting to schedule action that is already scheduled");

                int index = schedules.BinarySearch(schedule, comparer);

                if (index < 0) {
                    index = ~index;
                    schedules.Insert(index, schedule);
                }
                else
                    schedules.Insert(index, schedule);

                schedule.IsScheduled = true;

                Global.Logger.Debug(string.Format("Schedule inserted at position {0}.", index));

                if (index == 0)
                    SetNextActionTime();
            }
        }

        public bool Remove(ISchedule schedule) {
            lock (schedulesLock) {
                if (!schedule.IsScheduled) {
                    Global.Logger.Warn("Attempting to remove unscheduled action");
                    return false;
                }

                if (schedules.Remove(schedule)) {
                    schedule.IsScheduled = false;
                    Global.Logger.Debug("Schedule removed");
                    SetNextActionTime();
                    return true;
                }

                return false;
            }
        }

        // call back for the timer function
        private void DispatchAction(object obj) // obj ignored
        {            
            lock (schedulesLock) {
                if (schedules.Count == 0 || Paused) {
                    Global.Logger.Debug("In DispatchAction but no schedules");
                    return;
                }

                // Get the schedule that is supposed to fire
                ISchedule next = schedules[0];
                schedules.RemoveAt(0);
                next.IsScheduled = false;

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

                Global.Logger.Debug(string.Format("Action dispatched. Delta {0} ms", (next.Time - SystemClock.Now).TotalMilliseconds));

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
                Global.Logger.Debug("No actions available.");
                SetTimer(Timeout.Infinite);
                return;
            }

            TimeSpan ts = schedules[0].Time.Subtract(DateTime.UtcNow);
            int ms = Math.Max(0, (int)ts.TotalMilliseconds);
            SetTimer(ms);            
        }
    }
}