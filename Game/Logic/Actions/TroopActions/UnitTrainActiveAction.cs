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
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class UnitTrainActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;

        private readonly ushort count;

        private readonly uint structureId;

        private readonly ushort type;

        private Resource cost;

        private int timePerUnit;

        public UnitTrainActiveAction(uint cityId, uint structureId, ushort type, ushort count)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
            this.count = count;
        }

        public UnitTrainActiveAction(uint id,
                                     DateTime beginTime,
                                     DateTime nextTime,
                                     DateTime endTime,
                                     int workerType,
                                     byte workerIndex,
                                     ushort actionCount,
                                     Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
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
            timePerUnit = int.Parse(properties["time_per_unit"]);
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
            if (!World.Current.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectStructureNotFound;
            }

            var template = structure.City.Template[type];

            if (template == null)
            {
                return Error.Unexpected;
            }

            var unitLvl = template.Lvl;
            var unitTime = Ioc.Kernel.Get<UnitFactory>().GetTime(type, unitLvl);

            cost = Formula.Current.UnitTrainCost(structure.City, type, unitLvl);
            Resource totalCost = cost * count;
            ActionCount = (ushort)(count + count / Formula.Current.GetXForOneCount(structure.Technologies));

            if (!structure.City.Resource.HasEnough(totalCost))
            {
                return Error.ResourceNotEnough;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(totalCost);
            structure.City.EndUpdate();

            timePerUnit = (int)CalculateTime(Formula.Current.TrainTime(unitTime, structure.Lvl, structure.Technologies));

            // add to queue for completion
            nextTime = DateTime.UtcNow.AddSeconds(timePerUnit);
            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(timePerUnit * ActionCount);

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
            IStructure structure;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (Ioc.Kernel.Get<UnitFactory>().GetName(type, 1) == null)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.City.DefaultTroop.BeginUpdate();
                structure.City.DefaultTroop.AddUnit(city.HideNewUnits ? FormationType.Garrison : FormationType.Normal,
                                                    type,
                                                    1);
                structure.City.DefaultTroop.EndUpdate();

                --ActionCount;
                if (ActionCount == 0)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                nextTime = DateTime.UtcNow.AddSeconds(timePerUnit);
                endTime = DateTime.UtcNow.AddSeconds(timePerUnit * ActionCount);

                StateChange(ActionState.Rescheduled);
            }
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            IStructure structure;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (!wasKilled)
                {
                    int xfor1 = Formula.Current.GetXForOneCount(structure.Technologies);
                    int totalordered = count + count / xfor1;
                    int totaltrained = totalordered - ActionCount;
                    int totalpaidunit = totaltrained - (totaltrained - 1) / xfor1;
                    int totalrefund = count - totalpaidunit;
                    Resource totalCost = cost * totalrefund;

                    structure.City.BeginUpdate();
                    structure.City.Resource.Add(Formula.Current.GetActionCancelResource(BeginTime, totalCost));
                    structure.City.EndUpdate();
                }

                StateChange(ActionState.Failed);
            }
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
                                new XmlKvPair("type", type), new XmlKvPair("city_id", cityId),
                                new XmlKvPair("structure_id", structureId), new XmlKvPair("wood", cost.Wood),
                                new XmlKvPair("crop", cost.Crop), new XmlKvPair("iron", cost.Iron),
                                new XmlKvPair("gold", cost.Gold), new XmlKvPair("labor", cost.Labor),
                                new XmlKvPair("count", count), new XmlKvPair("time_per_unit", timePerUnit)
                        });
            }
        }

        #endregion
    }
}