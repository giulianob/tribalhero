#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Module;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class ResourceSellAction : ScheduledActiveAction
    {
        private const int TRADE_SIZE = 100;
        private const int MAX_TRADE = 15;
        private readonly uint cityId;

        private readonly ushort price;
        private readonly ushort quantity;
        private readonly ResourceType resourceType;
        private readonly uint structureId;

        public ResourceSellAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.price = price;
            this.quantity = quantity;
            this.resourceType = resourceType;
        }

        public ResourceSellAction(uint id,
                                  DateTime beginTime,
                                  DateTime nextTime,
                                  DateTime endTime,
                                  int workerType,
                                  byte workerIndex,
                                  ushort actionCount,
                                  Dictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            quantity = ushort.Parse(properties["quantity"]);
            price = ushort.Parse(properties["price"]);
            resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), properties["resource_type"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.ResourceSell;
            }
        }

        public override Error Execute()
        {
            City city;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;

            Market market;
            Resource cost;

            if (quantity == 0 || quantity%TRADE_SIZE != 0 || (quantity/TRADE_SIZE) > MAX_TRADE)
                return Error.MarketInvalidQuantity;

            switch(resourceType)
            {
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
                    return Error.Unexpected;
            }

            if (!structure.City.Resource.HasEnough(cost))
                return Error.ResourceNotEnough;

            if (!market.Sell(quantity, price))
                return Error.MarketPriceChanged;

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.TradeTime(structure, quantity)));
            BeginTime = DateTime.UtcNow;

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;
                StateChange(ActionState.Failed);
            }
        }

        public override void Callback(object custom)
        {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.City.BeginUpdate();
                structure.City.Resource.Add(0, (int)Math.Round(price*(quantity/TRADE_SIZE)*(1.0 - Formula.MarketTax(structure))), 0, 0, 0);
                structure.City.EndUpdate();

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
                                                        new XmlKvPair("resource_type", resourceType.ToString()), new XmlKvPair("price", price),
                                                        new XmlKvPair("quantity", quantity)
                                                });
            }
        }

        #endregion
    }
}