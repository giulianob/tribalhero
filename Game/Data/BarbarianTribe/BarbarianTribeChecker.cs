using System;
using Game.Logic;

namespace Game.Data.BarbarianTribe
{
    public class BarbarianTribeChecker : ISchedule
    {
        private TimeSpan interval;
        private readonly IBarbarianTribeManager manager;
        private readonly IScheduler scheduler;

        public BarbarianTribeChecker(IBarbarianTribeManager manager, IScheduler scheduler)
        {
            this.manager = manager;
            this.scheduler = scheduler;
        }

        /// <summary>
        /// Start the schedule checker for idle settlement
        /// </summary>
        /// <param name="interval">Interval between each run</param>       
        public void Start(TimeSpan interval)
        {
            this.interval = interval;
            Time = DateTime.UtcNow;
            scheduler.Put(this);
        }

        #region Implementation of ISchedule

        public bool IsScheduled { get; set; }

        public DateTime Time { get; private set; }

        public void Callback(object custom)
        {
            manager.RelocateAsNeeded();
            Time = DateTime.UtcNow.Add(interval);
            scheduler.Put(this);
        }

        #endregion
    }
}
