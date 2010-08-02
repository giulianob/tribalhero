#region Directives

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class StructureChangeAction : ScheduledActiveAction {
        private readonly uint type;
        private readonly byte lvl;
        private readonly uint cityId;
        private readonly uint structureId;
        private Resource cost;

        public StructureChangeAction(uint cityId, uint structureId, uint type, byte lvl) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
            this.lvl = lvl;
        }

        public StructureChangeAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                     byte workerIndex, ushort actionCount, IDictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            lvl = byte.Parse(properties["lvl"]);
            type = uint.Parse(properties["type"]);
            cost = new Resource(int.Parse(properties["crop"]), int.Parse(properties["gold"]), int.Parse(properties["iron"]), int.Parse(properties["wood"]), int.Parse(properties["labor"]));
        }

        public override Error Validate(string[] parms) {
            if (type == uint.Parse(parms[0]) && lvl == uint.Parse(parms[1]))
                return Error.OK;
            return Error.ACTION_INVALID;
        }

        public override Error Execute() {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            cost = Formula.StructureCost(structure.City, type, lvl);
            if (cost == null)
                return Error.OBJECT_NOT_FOUND;

            if (!structure.City.Resource.HasEnough(cost))
                return Error.RESOURCE_NOT_ENOUGH;

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            endTime = DateTime.UtcNow.AddSeconds(Formula.BuildTime(StructureFactory.GetTime((ushort)type, lvl), city.MainBuilding.Lvl, structure.Technologies));
            beginTime = DateTime.UtcNow;

            return Error.OK;
        }

        private void InterruptCatchAll(bool wasKilled) {
            City city;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

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

        public override ActionType Type {
            get { return ActionType.STRUCTURE_CHANGE; }
        }

        #region ISchedule Members

        public override void Callback(object custom) {
            City city;
            Structure structure;

            // Block structure
            using (new MultiObjectLock(cityId, structureId, out city, out structure)) {
                if (!IsValid())
                    return;

                structure.BeginUpdate();
                structure.IsBlocked = true;
                structure.EndUpdate();
            }

            structure.City.Worker.Remove(structure, ActionInterrupt.CANCEL, new GameAction[] { this });

            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                structure.BeginUpdate();
                StructureFactory.GetStructure(structure, (ushort) type, lvl, false);
                structure.Technologies.Parent = structure.City.Technologies;
                InitFactory.InitGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                structure.IsBlocked = false;
                structure.EndUpdate();

                StateChange(ActionState.COMPLETED);
            }
        }

        #endregion

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                        new XMLKVPair("type", type), 
                                                        new XMLKVPair("lvl", lvl),
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