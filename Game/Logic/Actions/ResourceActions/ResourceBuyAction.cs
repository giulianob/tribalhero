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
        private const int TradeSize = 100;
        private const int MaxTrade = 15;
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

        public ResourceBuyAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
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

            if (quantity == 0 || quantity%TradeSize != 0 || quantity/TradeSize > 15)
                return Error.MARKET_INVALID_QUANTITY;

            switch (resourceType) {
                case ResourceType.Crop:
                    if (!Market.Crop.Buy(quantity, price))
                        return Error.MARKET_PRICE_CHANGED;
                    break;
                case ResourceType.Wood:
                    if (!Market.Wood.Buy(quantity, price))
                        return Error.MARKET_PRICE_CHANGED;
                    break;
                case ResourceType.Iron:
                    if (!Market.Iron.Buy(quantity, price))
                        return Error.MARKET_PRICE_CHANGED;
                    break;
            }

            Resource cost = new Resource(0,
                                         (int)
                                         Math.Round(price*(quantity/TradeSize)*(1.0 + Formula.MarketTax(structure))), 0,
                                         0, 0);
            if (!structure.City.Resource.HasEnough(cost)) {
                Market.Crop.Supply(quantity);
                return Error.RESOURCE_NOT_ENOUGH;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            endTime = DateTime.Now.AddSeconds(Formula.TradeTime(structure));
            beginTime = DateTime.Now;

            return Error.OK;
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
                    case ResourceType.Crop:
                        structure.City.Resource.Crop.Add(quantity);
                        break;
                    case ResourceType.Wood:
                        structure.City.Resource.Wood.Add(quantity);
                        break;
                    case ResourceType.Iron:
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

        public override void Interrupt(ActionInterrupt state) {
            Global.Scheduler.del(this);
            switch (state) {
                case ActionInterrupt.CANCEL:
                    break;
            }
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new XMLKVPair[] {
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