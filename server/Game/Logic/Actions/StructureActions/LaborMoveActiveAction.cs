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

#endregion

namespace Game.Logic.Actions
{
    public class LaborMoveActiveAction : ScheduledActiveAction
    {
        private readonly Formula formula;

        private readonly Procedure procedure;

        private uint cityId;

        private bool cityToStructure;

        private uint structureId;

        private readonly ILocker locker;

        private readonly IGameObjectLocator gameObjectLocator;

        public LaborMoveActiveAction(Formula formula, Procedure procedure, ILocker locker, IGameObjectLocator gameObjectLocator)
        {
            this.formula = formula;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
        }

        public LaborMoveActiveAction(uint cityId, uint structureId, bool cityToStructure, ushort count, Formula formula, Procedure procedure, ILocker locker, IGameObjectLocator gameObjectLocator)
            : this(formula, procedure, locker, gameObjectLocator)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.cityToStructure = cityToStructure;
            ActionCount = count;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
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
            ICity city;
            IStructure structure;
            if (!gameObjectLocator.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            // Calculate move time before transferring laborers so they are considered when calculating the value
            int moveTime = formula.LaborMoveTime(structure, ActionCount, cityToStructure);

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
                procedure.RecalculateCityResourceRates(structure.City);                
                structure.City.EndUpdate();
            }

            // add to queue for completion
            BeginTime = DateTime.UtcNow;            
            
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(moveTime));
            
            return Error.Ok;
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            });
        }

        public override void UserCancelled()
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
                    procedure.RecalculateCityResourceRates(structure.City);
                    structure.City.EndUpdate();
                }

                StateChange(ActionState.Failed);
            });
        }

        public override Error Validate(string[] parms)
        {
            ICity city;
            IStructure structure;
            if (!gameObjectLocator.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }
            if (cityToStructure)
            {
                if (ActionCount > formula.LaborMoveMax(structure))
                {
                    return Error.ActionCountInvalid;
                }
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

                if (cityToStructure)
                {
                    structure.BeginUpdate();
                    structure.Stats.Labor += ActionCount;
                    structure.EndUpdate();

                    structure.City.BeginUpdate();
                    procedure.RecalculateCityResourceRates(structure.City);
                    structure.City.EndUpdate();
                }
                else
                {
                    structure.City.BeginUpdate();
                    structure.City.Resource.Labor.Add(ActionCount);
                    structure.City.EndUpdate();
                }

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
                                new XmlKvPair("city_to_structure", cityToStructure), new XmlKvPair("city_id", cityId),
                                new XmlKvPair("structure_id", structureId)
                        });
            }
        }

        public override Error SystemCancelable
        {
            get
            {
                return Error.Ok;
            }
        }

        #endregion
    }
}