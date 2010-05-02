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

        public ForestDepleteAction(Forest forest, DateTime time) {
            Forest = forest;
            Time = time;
        }

        public void Callback(object custom) {
            
            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { Forest.ObjectId }, Global.Forests)) {
                List<Structure> camps = new List<Structure>(Forest);

                foreach (Structure obj in camps) {
                    Forest.BeginUpdate();
                    Forest.RemoveLumberjack(obj);
                    Forest.EndUpdate();

                    obj.City.BeginUpdate();
                    // Remove wood production rate
                    obj.City.Resource.Wood.Rate -= (int) obj["Rate"];
                    // Add labor back to city
                    obj.City.Resource.Labor.Add(obj.Stats.Labor);
                    obj.City.EndUpdate();

                    // Destroy structure                    
                    Global.World.Remove(obj);                    
                    obj.City.Remove(obj);
                }

                Global.Forests.RemoveForest(Forest);
            }

        }
    }
}
