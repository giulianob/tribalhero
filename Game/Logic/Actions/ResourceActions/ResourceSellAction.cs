using System;
using System.Collections.Generic;
using System.Text;
using Game.Logic;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Database;
using Game.Map;
using Game.Module;

namespace Game.Logic.Actions {
    class ResourceSellAction : ScheduledActiveAction {
        const int TradeSize = 100;
        const int MaxTrade = 15;
        uint cityId;
        uint structureId;
        
        ushort price;
        ushort quantity;
        ResourceType resourceType;

        public ResourceSellAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.price = price;
            this.quantity = quantity;
            this.resourceType = resourceType;
        }

        public ResourceSellAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType, byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        #region IAction Members

        public override Error execute() {

            City city;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            Market market = null;
            Resource cost = null;

            if (quantity == 0 || quantity % TradeSize != 0 || (quantity / TradeSize) > MaxTrade) {
                return Error.MARKET_INVALID_QUANTITY;
            }

            switch (resourceType) {
                case ResourceType.Crop:
                    market = Market.Crop;
                    cost = new Resource(quantity, 0, 0, 0, 0);
                    break;
                case ResourceType.Wood:
                    market = Market.Wood;
                    cost = new Resource(0, 0, 0, quantity, 0);
                    break;
                case ResourceType.Iron:
                    cost = new Resource(0, 0, quantity, 0, 0);
                    market = Market.Iron;
                    break;
                default:
                    return Error.UNEXPECTED;
            }

            if (cost == null) return Error.UNEXPECTED;

            if (!structure.City.Resource.HasEnough(cost)) return Error.RESOURCE_NOT_ENOUGH;

            if (!market.Sell(quantity, price)) return Error.MARKET_PRICE_CHANGED;

            structure.City.Resource.Subtract(cost);

            endTime = DateTime.Now.AddSeconds(Formula.TradeTime(structure));
            beginTime = DateTime.Now;

            Global.dbManager.Save(structure.City);

            return Error.OK;
        }

        public override void callback(object custom) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!isValid()) return;

                if (!city.tryGetStructure(structureId, out structure)) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                structure.City.Resource.Add(0, (int)Math.Round(price * (quantity / TradeSize) * (1.0 - Formula.MarketTax(structure))), 0, 0, 0);
                Global.dbManager.Save(structure.City);
                stateChange(ActionState.COMPLETED);
            }
        }

        public override ActionType Type {
            get { return ActionType.RESOURCE_BUY; }
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        #endregion

        public override void interrupt(ActionInterrupt state) {
            Global.Scheduler.del(this);
            switch (state) {
                case ActionInterrupt.CANCEL:
                    break;
            }
        }

        #region IPersistable

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                    new XMLKVPair("city_id", cityId),
                    new XMLKVPair("structure_id", structureId)
                });
            }
        }

        #endregion
    }
}
