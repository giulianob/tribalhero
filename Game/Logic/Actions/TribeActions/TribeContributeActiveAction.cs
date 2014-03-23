#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class TribeContributeActiveAction : ScheduledActiveAction
    {
        private uint cityId;

        private readonly Formula formula;

        private readonly IGameObjectLocator locator;

        private readonly ILocker locker;

        private Resource resource;

        private uint structureId;

        public TribeContributeActiveAction(IGameObjectLocator locator,
                                           Formula formula,
                                           ILocker locker)
        {
            this.locator = locator;
            this.formula = formula;
            this.locker = locker;
        }

        public TribeContributeActiveAction(uint cityId,
                                           uint structureId,
                                           Resource resource,
                                           IGameObjectLocator locator,
                                           Formula formula,
                                           ILocker locker)
            : this(locator, formula, locker)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.resource = new Resource(resource);
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            resource = new Resource(int.Parse(properties["crop"]),
                                    int.Parse(properties["gold"]),
                                    int.Parse(properties["iron"]),
                                    int.Parse(properties["wood"]),
                                    int.Parse(properties["labor"]));
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
                return ActionType.TribeContributeActive;
            }
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;

            if (!locator.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            // Make sure we aren't exceeding our trade capacity
            if (!formula.GetContributeCapacity(structure).HasEnough(resource))
            {
                return Error.ResourceExceedTradeLimit;
            }

            // Gotta send something
            if (resource.Total == 0)
            {
                return Error.ResourceNotEnough;
            }

            if (!city.Resource.HasEnough(resource))
            {
                return Error.ResourceNotEnough;
            }

            if (!city.Owner.IsInTribe)
            {
                return Error.TribesmanNotPartOfTribe;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(resource);
            structure.City.EndUpdate();

            endTime = SystemClock.Now.AddSeconds(CalculateTime(formula.TradeTime(structure, resource)));
            BeginTime = SystemClock.Now;

            return Error.Ok;
        }

        public override void UserCancelled()
        {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll(wasKilled);
        }

        private void RefundResource(ICity city)
        {
            city.BeginUpdate();
            city.Resource.Add(formula.GetActionCancelResource(BeginTime, resource));
            city.EndUpdate();
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
                    RefundResource(city);
                }

                StateChange(ActionState.Failed);
            });
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

                var tribe = city.Owner.IsInTribe ? city.Owner.Tribesman.Tribe : null;

                if (tribe == null || city == null)
                {
                    if (city != null)
                    {
                        RefundResource(city);
                    }

                    StateChange(ActionState.Failed);
                    return;
                }

                if (tribe.Contribute(city.Owner.PlayerId, resource) != Error.Ok)
                {
                    RefundResource(city);
                    StateChange(ActionState.Failed);
                    return;
                }

                StateChange(ActionState.Completed);
            });
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), new XmlKvPair("structure_id", structureId),
                                new XmlKvPair("crop", resource.Crop), new XmlKvPair("gold", resource.Gold),
                                new XmlKvPair("iron", resource.Iron), new XmlKvPair("wood", resource.Wood),
                                new XmlKvPair("labor", resource.Labor)
                        });
            }
        }

        #endregion
    }
}