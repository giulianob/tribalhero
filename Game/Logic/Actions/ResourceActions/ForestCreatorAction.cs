using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Logic.Procedures;
using Game.Util;

namespace Game.Logic.Actions.ResourceActions {
    public class ForestCreatorAction : ISchedule {

        public DateTime Time { get; private set; }

        public ForestCreatorAction() {            
            Time = SystemClock.Now.AddMinutes(5);
        }

        public void Callback(object custom) {
            
            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { }, Global.Forests)) {
                
                for (byte i = 0; i < Global.Forests.ForestDeletedCount.Length; i++) {
                    Global.Forests.CreateForest(i, Formula.GetMaxForestCapacity(i), Formula.GetMaxForestRate(i));
                }

                // Reschedule ourselves
                Time = SystemClock.Now.AddMinutes(5);
                Global.Scheduler.Put(this);
            }
        }
    }
}
