using System;
using Game.Logic;
using Persistance;

namespace Game.Data.BarbarianTribe
{
    public class BarbarianTribeChecker : ISchedule
    {
        private TimeSpan interval;
        private readonly IBarbarianTribeManager manager;
        private readonly IScheduler scheduler;

        private readonly IDbManager dbManager;

        public BarbarianTribeChecker(IBarbarianTribeManager manager, IScheduler scheduler, IDbManager dbManager)
        {
            this.manager = manager;
            this.scheduler = scheduler;
            this.dbManager = dbManager;
        }

        /// <summary>
        /// Start the schedule checker for idle barbarian tribes
        /// </summary>        
        public void Start(TimeSpan intervalSpan)
        {
            this.interval = intervalSpan;
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
