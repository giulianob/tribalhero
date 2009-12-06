using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Game.Database;

namespace Game.Logic {
    public class Scheduler {
        object schedulesLock = new object();
        ScheduleComparer comparer = new ScheduleComparer();
        List<ISchedule> schedules = new List<ISchedule>();
        Timer timer;
        bool paused = false;
        public bool Paused {
            get { return paused; }
        }

        public Scheduler() {
            timer = new Timer(new TimerCallback(DispatchAction), // main timer
                                            null,
                                            Timeout.Infinite,
                                            Timeout.Infinite);
        }

        public void pause() {
            lock (schedulesLock) {
                paused = true;
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void resume() {
            lock (schedulesLock) {
                paused = false;
                SetNextActionTime();
            }
        }

        public void put(ISchedule schedule) {
            lock (schedulesLock) {
                int index = schedules.BinarySearch(schedule, comparer);

                if (index < 0) {
                    index = ~index;
                    schedules.Insert(index, schedule);
                }
                else {
                    schedules.Insert(index, schedule);
                }

                if (index == 0) 
                    SetNextActionTime();
            }
        }

        public void del(ISchedule schedule) {
            lock (schedulesLock) {
                if (schedules.Remove(schedule)) {
                    SetNextActionTime();
                }
            }
        }

        // call back for the timer function
        private void DispatchAction(object obj) // obj ignored
        {
            lock (schedulesLock) {
                if (schedules.Count == 0) 
                    return;
                ISchedule next = schedules[0];
                schedules.RemoveAt(0);
                ThreadPool.QueueUserWorkItem(new WaitCallback(DispatchThread), next);
                SetNextActionTime();
            }
        }

        private void DispatchThread(object obj) {
            (obj as ISchedule).callback(null);
        }

        // method to set the time when the timer should wake up to invoke the next schedule
        private void SetNextActionTime() {
            if (paused) return;

            if (schedules.Count == 0) {
                timer.Change(Timeout.Infinite, Timeout.Infinite); // this will put the timer to sleep
                return;
            }

            TimeSpan ts = schedules[0].Time.Subtract(DateTime.Now);
            if (ts < TimeSpan.Zero) ts = TimeSpan.Zero; // cannot be negative !
            timer.Change((int)ts.TotalMilliseconds, Timeout.Infinite); // invoke after the timespan
            //Global.Logger.Info("Next schedule in " + ts.TotalSeconds + " seconds.");
        }
    }
}
