#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Logic.Formulas;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class ResourceSellActiveAction : ScheduledActiveAction
    {
        private const int TRADE_SIZE = 100;

        private readonly uint cityId;

        private readonly ushort price;

        private readonly ushort quantity;

        private readonly ResourceType resourceType;

        private readonly uint structureId;

        public ResourceSellActiveAction(uint cityId,
                                        uint structureId,
                                        ushort price,
                                        ushort quantity,
                                        ResourceType resourceType)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.price = price;
            this.quantity = quantity;
            this.resourceType = resourceType;
        }

        public ResourceSellActiveAction(uint id,
                                        DateTime beginTime,
                                        DateTime nextTime,
                                        DateTime endTime,
                                        int workerType,
                                        byte workerIndex,
                                        ushort actionCount,
                                        Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            quantity = ushort.Parse(properties["quantity"]);
            price = ushort.Parse(properties["price"]);
            resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), properties["resource_type"]);
        }

        public override ConcurrencyType ActionConcurrency
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
                return ActionType.ResourceSellActive;
            }
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;

            if (!World.Current.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            Market market;
            Resource cost;

            if (!Formula.Current.MarketResourceSellable(structure).Contains(resourceType))
            {
                return Error.ResourceNotTradable;
            }

            if (quantity <= 0 || quantity % TRADE_SIZE != 0 || quantity > Formula.Current.MarketTradeQuantity(structure))
            {
                return Error.MarketInvalidQuantity;
            }

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
            {
                return Error.ResourceNotEnough;
            }

            if (!market.Sell(quantity, price))
            {
                return Error.MarketPriceChanged;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.Current.TradeTime(structure, quantity)));
            BeginTime = DateTime.UtcNow;

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

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
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
                    city.BeginUpdate();
                    Resource resource = new Resource();
                    switch(resourceType)
                    {
                        case ResourceType.Crop:
                            resource = new Resource(quantity, 0, 0, 0, 0);
                            Market.Crop.Consume(quantity);
                            break;
                        case ResourceType.Wood:
                            resource = new Resource(0, 0, 0, quantity, 0);
                            Market.Wood.Consume(quantity);
                            break;
                        case ResourceType.Iron:
                            resource = new Resource(0, 0, quantity, 0, 0);
                            Market.Iron.Consume(quantity);
                            break;
                    }

                    city.Resource.Add(Formula.Current.GetActionCancelResource(BeginTime, resource));
                    city.EndUpdate();
                }

                StateChange(ActionState.Failed);
            }
        }

        public override void Callback(object custom)
        {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
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

                structure.City.BeginUpdate();
                structure.City.Resource.Add(0, (int)Math.Round((double)(price * (quantity / TRADE_SIZE))), 0, 0, 0);
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