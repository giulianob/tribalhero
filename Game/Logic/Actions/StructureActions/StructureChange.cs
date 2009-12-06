using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Database;

namespace Game.Logic.Actions {
    class StructureChangeAction : ScheduledActiveAction {
        uint type;
        byte lvl;
        bool ignoreTime, ignoreCost;
        uint cityId;
        uint structureId;
        Resource cost;

        public StructureChangeAction(uint cityId, uint structureId, uint type, byte lvl, bool ignore_time, bool ignore_cost) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
            this.lvl = lvl;
            this.ignoreCost = ignore_cost;
            this.ignoreTime = ignore_time;
        }

        public StructureChangeAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType, byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            lvl = byte.Parse(properties["lvl"]);
            type = uint.Parse(properties["type"]);
            ignoreTime = bool.Parse(properties["ignore_time"]);
            ignoreCost = bool.Parse(properties["ignore_cost"]);
        }

        public override Error validate(string[] parms) {
            if (type == uint.Parse(parms[0]) && lvl == uint.Parse(parms[1])) return Error.OK;
            return Error.ACTION_INVALID;
        }

        public override Error execute() {

            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            if (!ignoreCost) {
                cost = Formula.StructureCost(structure.City, (uint)type, (byte)lvl);
                if (cost == null) {
                    return Error.OBJECT_NOT_FOUND;
                }

                if (!structure.City.Resource.HasEnough(cost)) {
                    return Error.RESOURCE_NOT_ENOUGH;
                }

                structure.City.Resource.Subtract(cost);
                Global.dbManager.Save(structure.City);
            }

            if (!ignoreTime) {
                this.endTime = DateTime.Now.AddSeconds(Formula.BuildTime(StructureFactory.getTime((ushort)type, lvl), structure.Technologies));
                this.beginTime = DateTime.Now;
            }
            else {
                structure.BeginUpdate();
                StructureFactory.getStructure(structure, (ushort)type, lvl, false);
                structure.Technologies.Parent = structure.City.Technologies;
                InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                this.stateChange(ActionState.COMPLETED);
            }

            return Error.OK;
        }

        public override void interrupt(ActionInterrupt state) {
            City city;
            using (new MultiObjectLock(cityId, out city)) {
                switch (state) {
                    case ActionInterrupt.KILLED:
                        if (!ignoreTime) Global.Scheduler.del(this);
                        stateChange(ActionState.FAILED);
                        break;
                    case ActionInterrupt.CANCEL:
                        if (!ignoreTime) {
                            Global.Scheduler.del(this);
                        }
                        city.Resource.Add(cost / 2);
                        stateChange(ActionState.INTERRUPTED);
                        break;
                }
            }

        }

        public override ActionType Type {
            get { return ActionType.STRUCTURE_CHANGE; }
        }

        #region ISchedule Members

        public override void callback(object custom) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!isValid()) return;

                if (!city.tryGetStructure(structureId, out structure)) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                structure.BeginUpdate();
                StructureFactory.getStructure(structure, (ushort)type, lvl, false);
                structure.Technologies.Parent = structure.City.Technologies;
                InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                structure.City.Worker.Remove(structure, ActionInterrupt.CANCEL, this);
                structure.EndUpdate();

                this.stateChange(ActionState.COMPLETED);
            }
        }

        #endregion

        #region IPersistable

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                    new XMLKVPair("type", type),
                    new XMLKVPair("lvl", lvl),
                    new XMLKVPair("ignore_time", ignoreTime),
                    new XMLKVPair("ignore_cost", ignoreCost),
                    new XMLKVPair("city_id", cityId),
                    new XMLKVPair("structure_id", structureId)
                });
            }
        }

        #endregion
    }
}
