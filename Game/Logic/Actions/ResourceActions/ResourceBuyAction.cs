#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Module;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class ResourceBuyAction : ScheduledActiveAction {
        private const int TRADE_SIZE = 100;
        private uint cityId;
        private uint structureId;
        private ushort price;
        private ushort quantity;
        private ResourceType resourceType;

        public ResourceBuyAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.price = price;
            this.quantity = quantity;
            this.resourceType = resourceType;
        }

        public ResourceBuyAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                 byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            quantity = ushort.Parse(properties["quantity"]);
            price = ushort.Parse(properties["price"]);
            resourceType = (ResourceType) Enum.Parse(typeof (ResourceType), properties["resource_type"]);
        }

        #region IAction Members

        public override Error Execute() {
            City city;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            if (quantity == 0 || quantity%TRADE_SIZE != 0 || quantity/TRADE_SIZE > 15)
                return Error.MARKET_INVALID_QUANTITY;

            switch (resourceType) {
                case ResourceType.CROP:
                    if (!Market.Crop.Buy(quantity, price))
                        return Error.MARKET_PRICE_CHANGED;
                    break;
                case ResourceType.WOOD:
                    if (!Market.Wood.Buy(quantity, price))
                        return Error.MARKET_PRICE_CHANGED;
                    break;
                case ResourceType.IRON:
                    if (!Market.Iron.Buy(quantity, price))
                        return Error.MARKET_PRICE_CHANGED;
                    break;
            }

            Resource cost = new Resource(0,
                                         (int)
                                         Math.Round(price*(quantity/TRADE_SIZE)*(1.0 + Formula.MarketTax(structure))), 0,
                                         0, 0);
            if (!structure.City.Resource.HasEnough(cost)) {
                Market.Crop.Supply(quantity);
                return Error.RESOURCE_NOT_ENOUGH;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : Formula.TradeTime(structure, quantity));
            beginTime = DateTime.UtcNow;

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
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                structure.City.BeginUpdate();
                switch (resourceType) {
                    case ResourceType.CROP:
                        structure.City.Resource.Crop.Add(quantity);
                        break;
                    case ResourceType.WOOD:
                        structure.City.Resource.Wood.Add(quantity);
                        break;
                    case ResourceType.IRON:
                        structure.City.Resource.Iron.Add(quantity);
                        break;
                }
                structure.City.EndUpdate();

                StateChange(ActionState.COMPLETED);
            }
        }

        public override ActionType Type {
            get { return ActionType.RESOURCE_BUY; }
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
                                                                new XMLKVPair("resource_type", resourceType.ToString()),
                                                                new XMLKVPair("price", price), new XMLKVPair("quantity", quantity)
                                                            });
            }
        }

        #endregion
    }
}