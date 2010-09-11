using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Actions.ResourceActions {
    public class ForestCreatorAction : ISchedule {

        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        public ForestCreatorAction() {
            Time = SystemClock.Now;
        }

        public void Callback(object custom) {
            
            using (new MultiObjectLock(Global.Forests)) {
                
                for (byte i = 0; i < Config.forest_count.Length; i++) {
                    byte lvl = (byte)(i + 1);
                    int delta = Config.forest_count[i] - Global.Forests.ForestCount[i];
                    
                    for (int j = 0; j < delta; j++)
                        Global.Forests.CreateForest(lvl, Formula.GetMaxForestCapacity(lvl), Formula.GetMaxForestRate(lvl));
                }

                // Reschedule ourselves
                Time = SystemClock.Now.AddMinutes(5);
                Global.Scheduler.Put(this);
            }
        }
    }
}
