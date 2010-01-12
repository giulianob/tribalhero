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
        private uint cityId;
        private uint structureId;

        public StructureUpgradeAction(uint cityId, uint structureId) {
            this.cityId = cityId;
            this.structureId = structureId;
        }

        public StructureUpgradeAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                      byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        #region IAction Members

        public override Error execute() {
            City city;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            // layout requirement
            if (
                !ReqirementFactory.getLayoutRequirement(structure.Type, (byte) (structure.Lvl + 1)).validate(
                     structure.City, structure.X, structure.Y))
                return Error.LAYOUT_NOT_FULLFILLED;

            Resource cost = StructureFactory.getCost(structure.Type, structure.Lvl + 1);

            if (cost == null)
                return Error.OBJECT_STRUCTURE_NOT_FOUND;

            if (!structure.City.Resource.HasEnough(cost))
                return Error.RESOURCE_NOT_ENOUGH;

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            endTime =
                DateTime.Now.AddSeconds(
                    Formula.BuildTime(StructureFactory.getTime(structure.Type, (byte) (structure.Lvl + 1)),
                                      structure.Technologies));
            beginTime = DateTime.Now;

            return Error.OK;
        }

        public override void callback(object custom) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!isValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                structure.BeginUpdate();
                StructureFactory.getStructure(structure, structure.Type, (byte) (structure.Lvl + 1), true);
                InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                Procedure.OnStructureUpgrade(structure);

                stateChange(ActionState.COMPLETED);
            }
        }

        public ActionState State {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override ActionType Type {
            get { return ActionType.STRUCTURE_UPGRADE; }
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        #endregion

        public override void interrupt(ActionInterrupt state) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!city.TryGetStructure(structureId, out structure)) {
                    Global.Scheduler.del(this);
                    stateChange(ActionState.FAILED);
                    return;
                }

                switch (state) {
                    case ActionInterrupt.KILLED:
                        Global.Scheduler.del(this);
                        stateChange(ActionState.FAILED);
                        break;
                    case ActionInterrupt.CANCEL:
                        Global.Scheduler.del(this);
                        Resource cost = StructureFactory.getCost(structure.Type, structure.Lvl + 1);

                        city.BeginUpdate();
                        city.Resource.Add(cost/2);
                        city.EndUpdate();

                        stateChange(ActionState.INTERRUPTED);
                        break;
                }
            }
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                      new XMLKVPair("city_id", cityId),
                                                      new XMLKVPair("structure_id", structureId)
                                                  });
            }
        }

        #endregion
    }
}