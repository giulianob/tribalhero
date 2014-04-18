#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class UnitTrainActiveAction : ScheduledActiveAction
    {
        private readonly UnitFactory unitFactory;

        private readonly ILocker locker;

        private readonly IWorld world;

        private readonly Formula formula;

        private uint cityId;

        private ushort count;

        private uint structureId;

        private ushort type;

        private Resource cost;

        public UnitTrainActiveAction(UnitFactory unitFactory,
                                     ILocker locker,
                                     IWorld world,
                                     Formula formula)
        {
            this.unitFactory = unitFactory;
            this.locker = locker;
            this.world = world;
            this.formula = formula;
        }

        public UnitTrainActiveAction(uint cityId,
                                     uint structureId,
                                     ushort type,
                                     ushort count,
                                     UnitFactory unitFactory,
                                     ILocker locker,
                                     IWorld world,
                                     Formula formula)
            : this(unitFactory, locker, world, formula)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
            this.count = count;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            type = ushort.Parse(properties["type"]);
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            cost = new Resource(int.Parse(properties["crop"]),
                                int.Parse(properties["gold"]),
                                int.Parse(properties["iron"]),
                                int.Parse(properties["wood"]),
                                int.Parse(properties["labor"]));
            count = ushort.Parse(properties["count"]);
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.Concurrent;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.UnitTrainActive;
            }
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;
            if (!world.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectStructureNotFound;
            }

            var template = structure.City.Template[type];

            if (template == null)
            {
                return Error.Unexpected;
            }

            var unitLvl = template.Lvl;
            
            cost = formula.UnitTrainCost(structure.City, type, unitLvl);
            Resource totalCost = cost * count;
            ActionCount = (ushort)(count + count / formula.GetXForOneCount(structure.Technologies));

            if (!structure.City.Resource.HasEnough(totalCost))
            {
                return Error.ResourceNotEnough;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(totalCost);
            structure.City.EndUpdate();

            ushort instantTrainCount = (ushort)Math.Min(formula.GetInstantTrainCount(structure),count);
            ActionCount -= instantTrainCount;

            structure.City.DefaultTroop.BeginUpdate();
            structure.City.DefaultTroop.AddUnit(structure.City.HideNewUnits ? FormationType.Garrison : FormationType.Normal, type, instantTrainCount);
            structure.City.DefaultTroop.EndUpdate();

            if (ActionCount == 0) // check if all units are instant trained
            {
                StateChange(ActionState.Completed);
                return Error.Ok;
            }

            var unitStats = unitFactory.GetUnitStats(type, unitLvl);
            var timePerUnit = CalculateTime(formula.TrainTime(structure.Lvl, 1, unitStats));
            var timeUntilComplete = CalculateTime(formula.TrainTime(structure.Lvl, ActionCount, unitStats));

            // add to queue for completion
            beginTime = SystemClock.Now;
            nextTime = SystemClock.Now.AddSeconds(timePerUnit);            
            endTime = SystemClock.Now.AddSeconds(timeUntilComplete);

            return Error.Ok;
        }

        public override Error Validate(string[] parms)
        {
            if (ushort.Parse(parms[0]) != type)
            {
                return Error.ActionInvalid;
            }

            return Error.Ok;
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

                if (unitFactory.GetName(type, 1) == null)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.City.DefaultTroop.BeginUpdate();
                structure.City.DefaultTroop.AddUnit(city.HideNewUnits ? FormationType.Garrison : FormationType.Normal, type, 1);
                structure.City.DefaultTroop.EndUpdate();

                --ActionCount;
                if (ActionCount == 0)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                var template = structure.City.Template[type];
                if (template == null)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                var unitStats = unitFactory.GetUnitStats(type, template.Lvl);
                var timePerUnit = CalculateTime(formula.TrainTime(structure.Lvl, 1, unitStats));
                var timeUntilComplete = CalculateTime(formula.TrainTime(structure.Lvl, ActionCount, unitStats));

                nextTime = SystemClock.Now.AddSeconds(timePerUnit);
                endTime = SystemClock.Now.AddSeconds(timeUntilComplete);

                StateChange(ActionState.Rescheduled);
            });
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

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (!wasKilled)
                {
                    int xfor1 = formula.GetXForOneCount(structure.Technologies);
                    int totalordered = count + count / xfor1;
                    int totaltrained = totalordered - ActionCount;
                    int totalpaidunit = totaltrained - (totaltrained / (xfor1 + 1));
                    int totalrefund = count - totalpaidunit;
                    Resource totalCost = cost * totalrefund;

                    structure.City.BeginUpdate();
                    structure.City.Resource.Add(formula.GetActionCancelResource(BeginTime, totalCost));
                    structure.City.EndUpdate();
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

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("type", type),
                                new XmlKvPair("city_id", cityId),
                                new XmlKvPair("structure_id", structureId),
                                new XmlKvPair("wood", cost.Wood),
                                new XmlKvPair("crop", cost.Crop),
                                new XmlKvPair("iron", cost.Iron),
                                new XmlKvPair("gold", cost.Gold),
                                new XmlKvPair("labor", cost.Labor),
                                new XmlKvPair("count", count)
                        });
            }
        }

        #endregion
    }
}