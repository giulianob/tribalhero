#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class ForestCampHarvestAction : ScheduledPassiveAction {
        private uint cityId;
        private uint forestId;

        public ForestCampHarvestAction(uint cityId, uint forestId) {
            isCancellable = true;
            this.forestId = forestId;
            this.cityId = cityId;
        }

        public ForestCampHarvestAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            isCancellable = true;
            forestId = uint.Parse(properties["forest_id"]);
            cityId = uint.Parse(properties["city_id"]);
        }

        public override Error Execute() {
            Forest forest;

            if (!Global.World.Forests.TryGetValue(forestId, out forest))
                return Error.OBJECT_NOT_FOUND;

            // add to queue for completion
            endTime = forest.DepleteTime.AddSeconds(30);
            beginTime = DateTime.UtcNow;

            return Error.OK;
        }

        public void Reschedule() {            
            MultiObjectLock.ThrowExceptionIfNotLocked(WorkerObject.City);

            Forest forest;
            if (!Global.World.Forests.TryGetValue(forestId, out forest)) {
                throw new Exception("Forest is missing");
            }
            
            Global.Scheduler.Remove(this);

            endTime = forest.DepleteTime.AddSeconds(30);
            StateChange(ActionState.RESCHEDULED);
        }

        public override void Callback(object custom) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City is missing");
            }

            using (new CallbackLock(Global.World.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.World.Forests)) {
                if (!IsValid()) return;

                endTime = DateTime.UtcNow.AddSeconds(30);
                StateChange(ActionState.RESCHEDULED);
            }
        }

        public override ActionType Type {
            get { return ActionType.FOREST_CAMP_HARVEST; }
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override void UserCancelled() {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City is missing");
            }

            using (new CallbackLock(Global.World.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.World.Forests)) {
                if (!IsValid())
                    return;
                
                Structure structure = (Structure)WorkerObject;                

                Forest forest;
                if (Global.World.Forests.TryGetValue(forestId, out forest)) {
                    // Recalculate the forest
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Give back the labors to the city
                city.BeginUpdate();
                city.Resource.Wood.Rate -= (int)structure["Rate"];
                city.Resource.Labor.Add(structure.Stats.Labor);
                city.EndUpdate();

                // Remove ourselves
                structure.BeginUpdate();
                Global.World.Remove(structure);
                city.ScheduleRemove(structure, false);
                structure.EndUpdate();

                StateChange(ActionState.FAILED);
            }
        }

        public override void WorkerRemoved(bool wasKilled) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City is missing");
            }

            using (new CallbackLock(Global.World.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.World.Forests)) {
                if (!IsValid())
                    return;                

                Structure structure = (Structure)WorkerObject;

                Forest forest;
                if (Global.World.Forests.TryGetValue(forestId, out forest)) {
                    // Recalculate the forest
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Give back the labors to the city
                city.BeginUpdate();
                city.Resource.Wood.Rate -= (int)structure["Rate"];
                city.Resource.Labor.Add(structure.Stats.Labor);
                city.EndUpdate();

                StateChange(ActionState.FAILED);
            }
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[]
                                                {                                                    
                                                    new XMLKVPair("forest_id", forestId),
                                                    new XMLKVPair("city_id", cityId)
                                                });
            }
        }

        #endregion
    }
}