#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class StructureUpgradeAction : ScheduledActiveAction {
        private readonly uint cityId;
        private readonly uint structureId;
        private Resource cost;

        public StructureUpgradeAction(uint cityId, uint structureId) {
            this.cityId = cityId;
            this.structureId = structureId;
        }

        public StructureUpgradeAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                      byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            cost = new Resource(int.Parse(properties["crop"]), int.Parse(properties["gold"]), int.Parse(properties["iron"]), int.Parse(properties["wood"]), int.Parse(properties["labor"]));
        }

        #region IAction Members

        public override Error Execute() {
            City city;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            // layout requirement
            if (!ReqirementFactory.GetLayoutRequirement(structure.Type, (byte)(structure.Lvl + 1)).Validate(structure, structure.Type, structure.X, structure.Y))
                return Error.LAYOUT_NOT_FULLFILLED;

            cost = StructureFactory.GetCost(structure.Type, structure.Lvl + 1);

            if (cost == null)
                return Error.OBJECT_STRUCTURE_NOT_FOUND;

            if (!structure.City.Resource.HasEnough(cost))
                return Error.RESOURCE_NOT_ENOUGH;

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : Formula.BuildTime(StructureFactory.GetTime(structure.Type, (byte)(structure.Lvl + 1)), city.MainBuilding.Lvl, structure.Technologies));
            beginTime = DateTime.UtcNow;

            return Error.OK;
        }

        public override void Callback(object custom) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                structure.BeginUpdate();
                StructureFactory.GetStructure(structure, structure.Type, (byte)(structure.Lvl + 1), true);
                InitFactory.InitGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                Procedure.OnStructureUpgrade(structure);

                StateChange(ActionState.COMPLETED);
            }
        }

        public override ActionType Type {
            get { return ActionType.STRUCTURE_UPGRADE; }
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        #endregion

        private void InterruptCatchAll(bool wasKilled) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {                    
                    StateChange(ActionState.FAILED);
                    return;
                }

                if (!wasKilled) {                    
                    city.BeginUpdate();
                    city.Resource.Add(cost/2);
                    city.EndUpdate();
                }

                StateChange(ActionState.FAILED);                                        
            }
        }

        public override void UserCancelled() {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled) {
            InterruptCatchAll(wasKilled);
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                        new XMLKVPair("city_id", cityId),
                                                        new XMLKVPair("structure_id", structureId),
                                                        new XMLKVPair("wood", cost.Wood),
                                                        new XMLKVPair("crop", cost.Crop),
                                                        new XMLKVPair("iron", cost.Iron),
                                                        new XMLKVPair("gold", cost.Gold),
                                                        new XMLKVPair("labor", cost.Labor),
                                                  });
            }
        }

        #endregion
    }
}