#region

using System;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic.Actions.ResourceActions
{
    public class ForestCreatorAction : ISchedule
    {
        public ForestCreatorAction()
        {
            Time = SystemClock.Now;
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        public void Callback(object custom)
        {
            using (Concurrency.Current.Lock(Global.World.Forests))
            {
                for (byte i = 0; i < Config.forest_count.Length; i++)
                {
                    var lvl = (byte)(i + 1);
                    int delta = Config.forest_count[i] - Global.World.Forests.ForestCount[i];

                    for (int j = 0; j < delta; j++)
                        Global.World.Forests.CreateForest(lvl, Formula.GetMaxForestCapacity(lvl), Formula.GetMaxForestRate(lvl));
                }

                // Reschedule ourselves
                Time = SystemClock.Now.AddMinutes(5);
                Global.Scheduler.Put(this);
            }
        }

        #endregion
    }
}