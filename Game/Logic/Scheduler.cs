using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Game.Database;

namespace Game.Logic {
    public class Scheduler {
        static object schedulesLock = new object();
        static ISchedule next;
        static ScheduleComparer comparer = new ScheduleComparer();
        static List<ISchedule> schedules = new List<ISchedule>();
        static Timer timer = new Timer(new TimerCallback(DispatchAction), // main timer
                                            null,
                                            Timeout.Infinite,
                                            Timeout.Infinite);
        static bool paused = false;
        public static bool Paused {
            get { return paused; }
        }

        public static void pause() {
            lock (schedulesLock) {
                paused = true;
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public static void resume() {
            lock (schedulesLock) {
                paused = false;
                SetNextActionTime();
            }
        }

        public static void put(ISchedule schedule) {
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

        public static void del(ISchedule schedule) {
            lock (schedulesLock) {
                if (schedules.Remove(schedule)) {
                    SetNextActionTime();
                }
            }
        }

        // call back for the timer function
        private static void DispatchAction(object obj) // obj ignored
        {
            lock (schedulesLock) {
                if (next == null)
                    return;
                if (schedules.Count == 0) 
                    return;                
                schedules.RemoveAt(0);
                ThreadPool.QueueUserWorkItem(new WaitCallback(DispatchThread), next);
                SetNextActionTime();
            }
        }

        private static void DispatchThread(object obj) {
            (obj as ISchedule).callback(null);
        }

        // method to set the time when the timer should wake up to invoke the next schedule
        private static void SetNextActionTime() {
            if (paused) return;

            if (schedules.Count == 0) {
                timer.Change(Timeout.Infinite, Timeout.Infinite); // this will put the timer to sleep
                return;
            }
            next = schedules[0];
            TimeSpan ts = next.Time.Subtract(DateTime.Now);
            if (ts < TimeSpan.Zero) ts = TimeSpan.Zero; // cannot be negative !
            timer.Change((int)ts.TotalMilliseconds, Timeout.Infinite); // invoke after the timespan
            //Global.Logger.Info("Next schedule in " + ts.TotalSeconds + " seconds.");
        }
    }
}
