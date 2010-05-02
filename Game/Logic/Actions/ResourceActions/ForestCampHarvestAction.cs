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
            this.forestId = forestId;
            this.cityId = cityId;
        }

        public ForestCampHarvestAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            forestId = uint.Parse(properties["forest_id"]);
            cityId = uint.Parse(properties["city_id"]);
        }

        public override Error Execute() {
            Forest forest;

            if (!Global.Forests.TryGetValue(forestId, out forest))
                return Error.OBJECT_NOT_FOUND;

            // add to queue for completion
            endTime = forest.DepleteTime;
            beginTime = DateTime.Now;

            return Error.OK;
        }

        public void Reschedule() {
            Forest forest;
            if (!Global.Forests.TryGetValue(forestId, out forest)) {
                throw new Exception("Forest is missing");
            }

            Global.Scheduler.Del(this);
            endTime = forest.DepleteTime;            
            StateChange(ActionState.RESCHEDULED);
        }

        public override void Callback(object custom) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City is missing");
            }

            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.Forests)) {
                if (!IsValid()) return;
                
                StateChange(ActionState.COMPLETED);
            }
        }

        public override ActionType Type {
            get { return ActionType.FOREST_CAMP_HARVEST; }
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override void Interrupt(ActionInterrupt state) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City is missing");
            }

            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.Forests)) {
                if (!IsValid()) return;

                switch (state) {               
                    case ActionInterrupt.CANCEL:
                    case ActionInterrupt.ABORT:
                    case ActionInterrupt.KILLED:
                        Global.Scheduler.Del(this);

                        Structure structure;
                        if (!city.TryGetStructure(((Structure)WorkerObject).ObjectId, out structure)) {
                            StateChange(ActionState.FAILED);
                            return;
                        }

                        Forest forest;
                        if (Global.Forests.TryGetValue(forestId, out forest)) {
                            // Recalculate the forest
                            forest.BeginUpdate();
                            forest.RemoveLumberjack(structure);
                            forest.RecalculateForest();
                            forest.EndUpdate();
                        }

                        // Give back the labors to the city
                        city.BeginUpdate();
                        city.Resource.Labor.Add(structure.Stats.Labor);
                        city.EndUpdate();

                        // Remove the camp
                        Global.World.Remove(structure);
                        city.Remove(structure);
                        StateChange(ActionState.FAILED);
                        break;
                }
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