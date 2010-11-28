using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Logic.Procedures;
using Game.Util;

namespace Game.Logic.Actions.ResourceActions {
    public class ForestDepleteAction : ISchedule {

        public Forest Forest { get; private set; }
        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        public ForestDepleteAction(Forest forest, DateTime time) {
            Forest = forest;
            Time = time;
        }

        public void Callback(object custom) {
            
            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { Forest.ObjectId }, Global.Forests)) {
                
                Global.Logger.Debug(string.Format("Destroying forest[{0}]", Forest.ObjectId));

                List<Structure> camps = new List<Structure>(Forest);

                foreach (Structure obj in camps) {
                    Forest.BeginUpdate();
                    Forest.RemoveLumberjack(obj);
                    Forest.EndUpdate();

                    obj.City.Owner.SendSystemMessage(null, "Forest depleted", "One of your lumbermill outposts have finished gathering wood from a forest. All laborers have returned to your city and are now idle.");

                    // Remove structure from city
                    obj.BeginUpdate();
                    Global.World.Remove(obj);
                    obj.City.ScheduleRemove(obj, false, true);
                    obj.EndUpdate();
                }

                Global.Forests.RemoveForest(Forest);
            }

        }
    }
}
