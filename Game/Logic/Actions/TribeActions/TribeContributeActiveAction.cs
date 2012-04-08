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
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class TribeContributeActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;
        private readonly uint structureId;
        private readonly IGameObjectLocator locator;
        private readonly Formula formula;
        private readonly ILocker locker;
        private readonly Resource resource;

        public TribeContributeActiveAction(uint cityId, uint structureId, Resource resource, IGameObjectLocator locator, Formula formula, ILocker locker)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.locator = locator;
            this.formula = formula;
            this.locker = locker;
            this.resource = new Resource(resource);

        }

        public TribeContributeActiveAction(uint id,
                                  DateTime beginTime,
                                  DateTime nextTime,
                                  DateTime endTime,
                                  int workerType,
                                  byte workerIndex,
                                  ushort actionCount,
                                  Dictionary<string, string> properties, IGameObjectLocator locator, Formula formula, ILocker locker)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.locator = locator;
            this.formula = formula;
            this.locker = locker;
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
                return Error.ObjectNotFound;

            // Make sure we aren't exceeding our trade capacity
            if (!formula.GetSendCapacity(structure).HasEnough(resource))
                return Error.ResourceExceedTradeLimit;

            // Gotta send something
            if (resource.Total == 0)
                return Error.ResourceNotEnough;

            if (!city.Resource.HasEnough(resource))
                return Error.ResourceNotEnough;

            if (!city.Owner.IsInTribe)
                return Error.TribesmanNotPartOfTribe;

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

        private void InterruptCatchAll(bool wasKilled) {
            ICity city;
            using (locker.Lock(cityId, out city)) {
                if (!IsValid())
                    return;

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (!wasKilled)
                {
                    RefundResource(city);
                }

                StateChange(ActionState.Failed);
            }
        }

        public override void Callback(object custom)
        {
            ICity city;
            ITribe tribe;

            using (locker.Lock(cityId, out city, out tribe))
            {
                if (!IsValid())
                    return;

                if(city==null)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                // what if tribe is not there anymore?
                if(tribe==null)
                {
                    RefundResource(city);
                    StateChange(ActionState.Failed);
                    return;
                }
                if(tribe.Contribute(city.Owner.PlayerId, resource)!=Error.Ok)
                {
                    RefundResource(city);
                    StateChange(ActionState.Failed);
                    return;                   
                }
                StateChange(ActionState.Completed);
            }
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
                                                        new XmlKvPair("crop", resource.Crop),
                                                        new XmlKvPair("gold", resource.Gold), new XmlKvPair("iron", resource.Iron),
                                                        new XmlKvPair("wood", resource.Wood), new XmlKvPair("labor", resource.Labor)
                                                });
            }
        }

        #endregion
    }
}