#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class UnitUpgradeActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;

        private readonly uint structureId;

        private readonly ILocker locker;

        private readonly IWorld world;

        private readonly Formula formula;

        private readonly UnitFactory unitFactory;

        public UnitUpgradeActiveAction(uint cityId, uint structureId, ushort type, ILocker locker, IWorld world, Formula formula, UnitFactory unitFactory)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            UnitType = type;
            this.locker = locker;
            this.world = world;
            this.formula = formula;
            this.unitFactory = unitFactory;
        }

        public UnitUpgradeActiveAction(uint id,
                                       DateTime beginTime,
                                       DateTime nextTime,
                                       DateTime endTime,
                                       int workerType,
                                       byte workerIndex,
                                       ushort actionCount,
                                       Dictionary<string, string> properties,
                                       ILocker locker,
                                       IWorld world,
                                       Formula formula,
                                       UnitFactory unitFactory)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.locker = locker;
            this.world = world;
            this.formula = formula;
            this.unitFactory = unitFactory;
            UnitType = ushort.Parse(properties["type"]);
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        public ushort UnitType { get; private set; }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.Normal;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.UnitUpgradeActive;
            }
        }

        public override Error Validate(string[] parms)
        {
            ICity city;
            IStructure structure;
            if (!world.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ActionInvalid;
            }

            if (ushort.Parse(parms[0]) != UnitType || byte.Parse(parms[1]) <= city.Template[UnitType].Lvl)
            {
                return Error.ActionInvalid;
            }

            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;
            if (!world.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            if (
                    city.Worker.ActiveActions.Values.Any(
                                                         action =>
                                                         action.Type == Type &&
                                                         UnitType == ((UnitUpgradeActiveAction)action).UnitType &&
                                                         action != this))
            {
                return Error.ActionAlreadyInProgress;
            }

            IBaseUnitStats unitStats = city.Template[UnitType];
            if (unitStats == null)
            {
                return Error.Unexpected;
            }

            Resource cost = formula.UnitUpgradeCost(city, UnitType, (byte)(unitStats.Lvl + 1));

            if (cost == null)
            {
                return Error.Unexpected;
            }

            if (!city.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            var upgradeTime = formula.BuildTime(unitFactory.GetUpgradeTime(UnitType, (byte)(unitStats.Lvl + 1)),
                                                city,
                                                structure.Technologies);
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(upgradeTime));
            BeginTime = DateTime.UtcNow;

            return Error.Ok;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(formula.GetActionCancelResource(BeginTime, unitFactory.GetUpgradeCost(UnitType,
                                                                                                            city.Template[UnitType]
                                                                                                                    .Lvl + 1)));
                    city.EndUpdate();
                }

                StateChange(ActionState.Failed);
            });
        }

        public override void UserCancelled()
        {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll(wasKilled);
        }

        public override void Callback(object custom)
        {
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.City.Template[UnitType] = unitFactory.GetUnitStats(UnitType, (byte)(structure.City.Template[UnitType].Lvl + 1));

                StateChange(ActionState.Completed);
            });
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("type", UnitType), new XmlKvPair("city_id", cityId),
                                new XmlKvPair("structure_id", structureId)
                        });
            }
        }

        #endregion
    }
}