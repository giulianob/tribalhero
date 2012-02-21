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
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    class ResourceBuyActiveAction : ScheduledActiveAction
    {
        private const int TRADE_SIZE = 100;
        private readonly uint cityId;
        private readonly ushort price;
        private readonly ushort quantity;
        private readonly ResourceType resourceType;
        private readonly uint structureId;

        public ResourceBuyActiveAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.price = price;
            this.quantity = quantity;
            this.resourceType = resourceType;
        }

        public ResourceBuyActiveAction(uint id,
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

        private Resource GetCost()
        {
            return new Resource(0, (int)Math.Round((double)(price * (quantity / TRADE_SIZE))), 0, 0, 0); 
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
                return ActionType.ResourceBuyActive;
            }
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;

            if (!World.Current.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;

            if (!Formula.Current.MarketResourceBuyable(structure).Contains(resourceType))
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
                    if (!Market.Crop.Buy(quantity, price))
                        return Error.MarketPriceChanged;
                    break;
                case ResourceType.Wood:
                    if (!Market.Wood.Buy(quantity, price))
                        return Error.MarketPriceChanged;
                    break;
                case ResourceType.Iron:
                    if (!Market.Iron.Buy(quantity, price))
                        return Error.MarketPriceChanged;
                    break;
            }

            var cost = GetCost();
            if (!structure.City.Resource.HasEnough(cost))
            {
                Market.Crop.Supply(quantity);
                return Error.ResourceNotEnough;
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

        private void InterruptCatchAll(bool wasKilled) {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city)) {
                if (!IsValid())
                    return;

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (!wasKilled) {
                    city.BeginUpdate();
                    switch (resourceType) {
                        case ResourceType.Crop:
                            Market.Crop.Supply(quantity);
                            break;
                        case ResourceType.Wood:
                            Market.Wood.Supply(quantity);
                            break;
                        case ResourceType.Iron:
                            Market.Iron.Supply(quantity);
                            break;
                    }
                    var cost = GetCost();
                    city.Resource.Add(Formula.Current.GetActionCancelResource(BeginTime, cost));
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
                    return;

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.City.BeginUpdate();
                switch(resourceType)
                {
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