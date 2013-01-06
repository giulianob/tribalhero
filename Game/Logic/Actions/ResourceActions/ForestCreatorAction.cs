#region

using System;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

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
            using (Concurrency.Current.Lock(World.Current.Forests))
            {
                for (byte i = 0; i < Config.forest_count.Length; i++)
                {
                    var lvl = (byte)(i + 1);
                    int delta = Config.forest_count[i] - World.Current.Forests.ForestCount[i];

                    for (int j = 0; j < delta; j++)
                    {
                        World.Current.Forests.CreateForest(lvl,
                                                           Formula.Current.GetMaxForestCapacity(lvl),
                                                           Formula.Current.GetMaxForestRate(lvl));
                    }
                }

                // Reschedule ourselves
                Time = SystemClock.Now.AddMinutes(5);
                Scheduler.Current.Put(this);
            }
        }

        #endregion
    }
}