#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Module;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class ResourceSendAction : ScheduledActiveAction {
        private readonly uint cityId;
        private readonly uint structureId;
        private readonly uint targetCityId;
        private readonly Resource resource;

        public ResourceSendAction(uint cityId, uint structureId, uint targetCityId, Resource resource) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.targetCityId = targetCityId;
            this.resource = new Resource(resource);
        }

        public ResourceSendAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                 byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            resource = new Resource(int.Parse(properties["crop"]), int.Parse(properties["gold"]), int.Parse(properties["iron"]), int.Parse(properties["wood"]), int.Parse(properties["labor"]));
        }

        #region IAction Members

        public override Error Execute() {
            City city, targetCity;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure)) {
                return Error.OBJECT_NOT_FOUND;
            }

            if (!Global.World.TryGetObjects(targetCityId, out targetCity)) {
                return Error.OBJECT_NOT_FOUND;
            }

            if( !Formula.GetSendCapacity(structure).HasEnough(resource) ) {
                return Error.RESOURCE_EXCEED_TRADE_LIMIT;
            }

            if (resource.Total == 0) {
                return Error.RESOURCE_NOT_ENOUGH;
            }

            if( !city.Resource.HasEnough(resource) ) {
                return Error.RESOURCE_NOT_ENOUGH;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(resource);
            structure.City.EndUpdate();

            endTime = SystemClock.Now.AddSeconds(Config.actions_instant_time ? 3 : Formula.SendTime(structure.TileDistance(targetCity.MainBuilding)));
            beginTime = SystemClock.Now;

            return Error.OK;
        }

        public override void UserCancelled() {            
        }

        public override void WorkerRemoved(bool wasKilled) {
            City city;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid()) return;
                StateChange(ActionState.FAILED);
            }
        }

        public override void Callback(object custom) {
            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                if (!IsValid())
                    return;
                // what if city is not there anymore?
                City target = cities[targetCityId];
                target.BeginUpdate();
                target.Resource.Add(resource);
                target.EndUpdate();
                
                City sender = cities[cityId];
                target.Owner.SendSystemMessage(sender.Owner, string.Format("{0} has sent you resources", sender.Name), string.Format("{0} has sent you {1}.", sender.Owner.Name, resource.ToNiceString()));

                StateChange(ActionState.COMPLETED);
            }
        }

        public override ActionType Type {
            get { return ActionType.RESOURCE_SEND; }
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        #endregion

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("structure_id", structureId),
                                                                new XMLKVPair("target_city_id", targetCityId),
                                                                new XMLKVPair("crop", resource.Crop),
                                                                new XMLKVPair("gold", resource.Gold), 
                                                                new XMLKVPair("iron", resource.Iron), 
                                                                new XMLKVPair("wood", resource.Wood),
                                                                new XMLKVPair("labor", resource.Labor)
                                                            });
            }
        }

        #endregion
    }
}