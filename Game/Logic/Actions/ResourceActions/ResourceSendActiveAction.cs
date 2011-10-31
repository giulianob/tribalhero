#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    class ResourceSendActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;
        private readonly Resource resource;
        private readonly uint structureId;
        private readonly uint targetCityId;

        public ResourceSendActiveAction(uint cityId, uint structureId, uint targetCityId, Resource resource)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.targetCityId = targetCityId;
            this.resource = new Resource(resource);
        }

        public ResourceSendActiveAction(uint id,
                                  DateTime beginTime,
                                  DateTime nextTime,
                                  DateTime endTime,
                                  int workerType,
                                  byte workerIndex,
                                  ushort actionCount,
                                  Dictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            resource = new Resource(int.Parse(properties["crop"]),
                                    int.Parse(properties["gold"]),
                                    int.Parse(properties["iron"]),
                                    int.Parse(properties["wood"]),
                                    int.Parse(properties["labor"]));
        }

        public override ConcurrencyType Concurrency
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
                return ActionType.ResourceSendActive;
            }
        }

        public override Error Execute()
        {
            City city, targetCity;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;

            if (!Global.World.TryGetObjects(targetCityId, out targetCity))
                return Error.ObjectNotFound;

            // Can't send to cities that are being deleted
            if (targetCity.Deleted != City.DeletedState.NotDeleted)
                return Error.ObjectNotFound;

            // Make sure we aren't exceeding our trade capacity
            if (!Formula.GetSendCapacity(structure).HasEnough(resource))
                return Error.ResourceExceedTradeLimit;

            // Gotta send something
            if (resource.Total == 0)
                return Error.ResourceNotEnough;

            if (!city.Resource.HasEnough(resource))
                return Error.ResourceNotEnough;

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(resource);
            structure.City.EndUpdate();

            endTime = SystemClock.Now.AddSeconds(CalculateTradeTime(structure, targetCity));
            BeginTime = SystemClock.Now;

            return Error.Ok;
        }

        public int CalculateTradeTime(Structure structure, City targetCity)
        {
            return (int)CalculateTime(Formula.SendTime(structure, SimpleGameObject.TileDistance(structure.X, structure.Y, targetCity.X, targetCity.Y)));
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(cityId, out city))
            {
                if (!IsValid())
                    return;
                StateChange(ActionState.Failed);
            }
        }

        public override void Callback(object custom)
        {
            Dictionary<uint, City> cities;
            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(out cities, cityId, targetCityId))
            {
                if (!IsValid())
                    return;
                // what if city is not there anymore?
                City target = cities[targetCityId];
                target.BeginUpdate();
                target.Resource.Add(resource);
                target.EndUpdate();

                City sender = cities[cityId];
                target.Owner.SendSystemMessage(sender.Owner,
                                               string.Format("{0} has sent you resources", sender.Name),
                                               string.Format("{0} has sent you {1}.", sender.Owner.Name, resource.ToNiceString()));

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
                                                        new XmlKvPair("target_city_id", targetCityId), new XmlKvPair("crop", resource.Crop),
                                                        new XmlKvPair("gold", resource.Gold), new XmlKvPair("iron", resource.Iron),
                                                        new XmlKvPair("wood", resource.Wood), new XmlKvPair("labor", resource.Labor)
                                                });
            }
        }

        #endregion
    }
}