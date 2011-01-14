#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Stats;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class UnitUpgradeAction : ScheduledActiveAction {
        private readonly uint cityId;
        private readonly uint structureId;

        public ushort UnitType { get; private set; }

        public UnitUpgradeAction(uint cityId, uint structureId, ushort type) {
            this.cityId = cityId;
            this.structureId = structureId;
            UnitType = type;
        }

        public UnitUpgradeAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                 byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            UnitType = ushort.Parse(properties["type"]);
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        public override Error Validate(string[] parms) {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ACTION_INVALID;

            if (ushort.Parse(parms[0]) != UnitType || byte.Parse(parms[1]) <= city.Template[UnitType].Lvl)
                return Error.ACTION_INVALID;

            return Error.OK;
        }

        public override Error Execute() {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            if (city.Worker.ActiveActions.Exists(action => (action.Type == Type) && (UnitType == ((UnitUpgradeAction) action).UnitType) && (action != this))) {
                return Error.ACTION_ALREADY_IN_PROGRESS;
            }

            BaseUnitStats unitStats = city.Template[UnitType];
            Resource cost = Formula.UnitUpgradeCost(city, UnitType, (byte)(unitStats.Lvl + 1));

            if (cost == null)
                return Error.OBJECT_NOT_FOUND;
            if (!city.Resource.HasEnough(cost))
                return Error.RESOURCE_NOT_ENOUGH;

            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.BuildTime(UnitFactory.GetUpgradeTime(UnitType, (byte)(unitStats.Lvl + 1)),city, structure.Technologies)));
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
                    city.Resource.Add(Formula.GetActionCancelResource(BeginTime, UnitFactory.GetUpgradeCost(UnitType, city.Template[UnitType].Lvl + 1)));
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
            get { return ActionType.UNIT_UPGRADE; }
        }

        #region ISchedule Members

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

                structure.City.Template[UnitType] = UnitFactory.GetUnitStats(UnitType, (byte) (structure.City.Template[UnitType].Lvl + 1));

                StateChange(ActionState.COMPLETED);
            }
        }

        #endregion

        #region IPersistable

        public override string Properties {
            get { return XMLSerializer.Serialize(new[] {new XMLKVPair("type", UnitType), new XMLKVPair("city_id", cityId), new XMLKVPair("structure_id", structureId)}); }
        }

        #endregion
    }
}