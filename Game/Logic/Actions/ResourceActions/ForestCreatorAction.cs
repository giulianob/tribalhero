#region

using System;
using Game.Data.Forest;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Logic.Actions.ResourceActions
{
    public class ForestCreatorAction : ISchedule
    {
        private readonly IDbManager dbManager;

        private readonly IForestManager forestManager;

        private readonly IScheduler scheduler;

        public ForestCreatorAction(IDbManager dbManager, IForestManager forestManager, IScheduler scheduler)
        {
            this.dbManager = dbManager;
            this.forestManager = forestManager;
            this.scheduler = scheduler;
            Time = SystemClock.Now;
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }

        public bool IsScheduled { get; set; }

        public void Callback(object custom)
        {
            using (dbManager.GetThreadTransaction())
            {
                forestManager.RegenerateForests();
            }

            // Reschedule ourselves
            Time = SystemClock.Now.AddMinutes(5);
            scheduler.Put(this);           
        }

        #endregion
    }
}