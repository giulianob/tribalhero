#region

using System;
using Game.Data;
using Game.Logic.Actions;
using Game.Module;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class MarketCommandsModule : CommandModule
    {
        private readonly IActionFactory actionFactory;

        private readonly ILocker locker;

        private readonly IStructureCsvFactory structureCsvFactory;

        public MarketCommandsModule(IActionFactory actionFactory, ILocker locker, IStructureCsvFactory structureCsvFactory)
        {
            this.actionFactory = actionFactory;
            this.locker = locker;
            this.structureCsvFactory = structureCsvFactory;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.MarketBuy, MarketBuy);
            processor.RegisterCommand(Command.MarketSell, MarketSell);
            processor.RegisterCommand(Command.MarketPrices, MarketGetPrices);
        }

        private void MarketGetPrices(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            reply.AddUInt16((ushort)Market.Crop.Price);
            reply.AddUInt16((ushort)Market.Wood.Price);
            reply.AddUInt16((ushort)Market.Iron.Price);
            session.Write(reply);
        }

        private void MarketBuy(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            ResourceType type;
            ushort quantity;
            ushort price;
            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                type = (ResourceType)packet.GetByte();
                quantity = packet.GetUInt16();
                price = packet.GetUInt16();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (obj != null)
                {
                    Error ret;
                    var rba = actionFactory.CreateResourceBuyActiveAction(cityId, objectId, price, quantity, type);
                    if (
                            (ret =
                             city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                  obj,
                                                  rba,
                                                  obj.Technologies)) == 0)
                    {
                        ReplySuccess(session, packet);
                    }
                    else
                    {
                        ReplyError(session, packet, ret);
                    }
                    return;
                }
                ReplyError(session, packet, Error.Unexpected);
            }
        }

        private void MarketSell(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            ResourceType type;
            ushort quantity;
            ushort price;
            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                type = (ResourceType)packet.GetByte();
                quantity = packet.GetUInt16();
                price = packet.GetUInt16();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (obj != null)
                {
                    var rsa = actionFactory.CreateResourceSellActiveAction(cityId, objectId, price, quantity, type);
                    var actionResult = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                   obj,
                                                   rsa,
                                                   obj.Technologies);
                    if (actionResult == 0)
                    {
                        ReplySuccess(session, packet);
                    }
                    else
                    {
                        ReplyError(session, packet, actionResult);
                    }
                    return;
                }
                ReplyError(session, packet, Error.Unexpected);
            }
        }
    }
}