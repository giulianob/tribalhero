using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Fighting;

namespace Game.Logic.Actions {
    class UnitUpgradeAction : ScheduledActiveAction {
        uint cityId;
        uint structureId;
        ushort type;

        public ushort UnitType {
            get { return type; }
        }
        public UnitUpgradeAction(uint cityId, uint structureId, ushort type) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
        }

        public UnitUpgradeAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType, byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            type = ushort.Parse(properties["type"]);            
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        public override Error validate(string[] parms) {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ACTION_INVALID;

            if (ushort.Parse(parms[0]) != this.type && byte.Parse(parms[1]) < structure.Lvl+1) return Error.ACTION_INVALID;
            return Error.OK;
        }

        public override Error execute() {

            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            if (city.Worker.ActiveActions.Exists(delegate(ActiveAction action) {
                return (action.Type == this.Type) && (UnitType == (action as UnitUpgradeAction).UnitType) && (action != this);
            })) {
                return Error.ACTION_ALREADY_IN_PROGRESS;
            }

            UnitStats unitStats = city.Template[type];
            Resource cost = UnitFactory.getUpgradeCost(type, unitStats.lvl + 1);

            if (cost == null) {
                return Error.OBJECT_NOT_FOUND;
            }
            if (!city.Resource.HasEnough(cost)) {
                return Error.RESOURCE_NOT_ENOUGH;
            }

            city.Resource.Subtract(cost);
            Global.dbManager.Save(city);
            endTime = DateTime.Now.AddSeconds(Formula.BuildTime(UnitFactory.getUpgradeTime(type, (byte)(unitStats.lvl + 1)), structure.Technologies));
            beginTime = DateTime.Now;

            return Error.OK;
        }

        public override void interrupt(ActionInterrupt state) {
            City city;            
            using (new MultiObjectLock(cityId, out city)) {
                Global.Scheduler.del(this);
                switch (state) {
                    case ActionInterrupt.CANCEL:
                        stateChange(ActionState.FAILED);
                        break;
                }
            }
        }

        public override ActionType Type {
            get { return ActionType.UNIT_UPGRADE; }
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

                structure.City.Template[type] = UnitFactory.getUnitStats(type, (byte)(structure.City.Template[type].lvl + 1));                

                this.stateChange(ActionState.COMPLETED);
            }
        }

        #endregion

        #region IPersistable

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                    new XMLKVPair("type", type),
                    new XMLKVPair("city_id", cityId),
                    new XMLKVPair("structure_id", structureId)
                });
            }
        }

        #endregion
    }
}
