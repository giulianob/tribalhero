#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    class LaborMoveActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;
        private readonly bool cityToStructure;
        private readonly uint structureId;

        public LaborMoveActiveAction(uint cityId, uint structureId, bool cityToStructure, ushort count)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.cityToStructure = cityToStructure;
            ActionCount = count;
        }

        public LaborMoveActiveAction(uint id,
                               DateTime beginTime,
                               DateTime nextTime,
                               DateTime endTime,
                               int workerType,
                               byte workerIndex,
                               ushort actionCount,
                               IDictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityToStructure = bool.Parse(properties["city_to_structure"]);
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
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
                return ActionType.LaborMoveActive;
            }
        }

        public override Error Execute()
        {
            City city;
            Structure structure;
            if (!World.Current.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;

            if (cityToStructure)
            {
                structure.City.BeginUpdate();
                structure.City.Resource.Labor.Subtract(ActionCount);
                structure.City.EndUpdate();
            }
            else
            {
                structure.BeginUpdate();
                structure.Stats.Labor -= ActionCount;
                structure.EndUpdate();

                structure.City.BeginUpdate();
                Procedure.Current.RecalculateCityResourceRates(structure.City);
                // labor got taken out immediately                
                structure.City.EndUpdate();
            }

            // add to queue for completion
            BeginTime = DateTime.UtcNow;

            if (cityToStructure)
                endTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.Current.LaborMoveTime(structure, (byte)ActionCount, structure.Technologies)));
            else
                endTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.Current.LaborMoveTime(structure, (byte)ActionCount, structure.Technologies) / 20));

            return Error.Ok;
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                    return;

                StateChange(ActionState.Failed);
            }
        }

        public override void UserCancelled()
        {
            City city;
            Structure structure;

            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (cityToStructure)
                {
                    structure.City.BeginUpdate();
                    structure.City.Resource.Labor.Add(ActionCount);
                    structure.City.EndUpdate();
                }
                else
                {
                    structure.BeginUpdate();
                    structure.Stats.Labor += ActionCount;
                    structure.EndUpdate();

                    structure.City.BeginUpdate();
                    Procedure.Current.RecalculateCityResourceRates(structure.City);
                    structure.City.EndUpdate();
                }

                StateChange(ActionState.Failed);
            }
        }

        public override Error Validate(string[] parms)
        {
            City city;
            Structure structure;
            if (!World.Current.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;
            if (cityToStructure)
            {
                if (ActionCount > Formula.Current.LaborMoveMax(structure))
                    return Error.ActionCountInvalid;
            }
            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            City city;
            Structure structure;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (cityToStructure)
                {
                    structure.BeginUpdate();
                    structure.Stats.Labor += ActionCount;
                    structure.EndUpdate();

                    structure.City.BeginUpdate();
                    Procedure.Current.RecalculateCityResourceRates(structure.City);
                    structure.City.EndUpdate();
                }
                else
                {
                    structure.City.BeginUpdate();
                    structure.City.Resource.Labor.Add(ActionCount);
                    structure.City.EndUpdate();
                }
                StateChange(ActionState.Completed);
                return;
            }
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                                                {
                                                        new XmlKvPair("city_to_structure", cityToStructure), new XmlKvPair("city_id", cityId),
                                                        new XmlKvPair("structure_id", structureId)
                                                });
            }
        }

        #endregion
    }
}