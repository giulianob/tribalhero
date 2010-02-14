#region

using System;
using Game.Data;
using Game.Logic.Actions;
using Game.Module;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm {
    public partial class Processor {
        public void CmdMarketGetPrices(Session session, Packet packet) {
            Packet reply = new Packet(packet);
            reply.AddUInt16((ushort) Market.Crop.Price);
            reply.AddUInt16((ushort) Market.Wood.Price);
            reply.AddUInt16((ushort) Market.Iron.Price);
            session.write(reply);
        }

        public void CmdMarketBuy(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ResourceType type;
            ushort quantity;
            ushort price;
            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                type = (ResourceType) packet.GetByte();
                quantity = packet.GetUInt16();
                price = packet.GetUInt16();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            if (type == ResourceType.Gold) {
                ReplyError(session, packet, Error.RESOURCE_NOT_TRADABLE);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.getCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (obj != null) {
                    Error ret;
                    ResourceBuyAction rba = new ResourceBuyAction(cityId, objectId, price, quantity, type);
                    if (
                        (ret =
                         city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, rba, obj.Technologies)) ==
                        0)
                        ReplySuccess(session, packet);
                    else
                        ReplyError(session, packet, ret);
                    return;
                }
                ReplyError(session, packet, Error.UNEXPECTED);
            }
        }

        public void CmdMarketSell(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ResourceType type;
            ushort quantity;
            ushort price;
            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                type = (ResourceType) packet.GetByte();
                quantity = packet.GetUInt16();
                price = packet.GetUInt16();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            if (type == ResourceType.Gold) {
                ReplyError(session, packet, Error.RESOURCE_NOT_TRADABLE);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.getCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (obj != null) {
                    Error ret;
                    ResourceSellAction rsa = new ResourceSellAction(cityId, objectId, price, quantity, type);
                    if (
                        (ret =
                         city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, rsa, obj.Technologies)) ==
                        0)
                        ReplySuccess(session, packet);
                    else
                        ReplyError(session, packet, ret);
                    return;
                }
                ReplyError(session, packet, Error.UNEXPECTED);
            }
        }
    }
}