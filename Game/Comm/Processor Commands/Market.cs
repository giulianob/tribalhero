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
            reply.addUInt16((ushort) Market.Crop.Price);
            reply.addUInt16((ushort) Market.Wood.Price);
            reply.addUInt16((ushort) Market.Iron.Price);
            session.write(reply);
        }

        public void CmdMarketBuy(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ResourceType type;
            ushort quantity;
            ushort price;
            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                type = (ResourceType) packet.getByte();
                quantity = packet.getUInt16();
                price = packet.getUInt16();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            if (type == ResourceType.Gold) {
                reply_error(session, packet, Error.RESOURCE_NOT_TRADABLE);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.getCity(cityId);

                if (city == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (obj != null) {
                    Error ret;
                    ResourceBuyAction rba = new ResourceBuyAction(cityId, objectId, price, quantity, type);
                    if (
                        (ret =
                         city.Worker.DoActive(StructureFactory.getActionWorkerType(obj), obj, rba, obj.Technologies)) ==
                        0)
                        reply_success(session, packet);
                    else
                        reply_error(session, packet, ret);
                    return;
                }
                reply_error(session, packet, Error.UNEXPECTED);
            }
        }

        public void CmdMarketSell(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ResourceType type;
            ushort quantity;
            ushort price;
            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                type = (ResourceType) packet.getByte();
                quantity = packet.getUInt16();
                price = packet.getUInt16();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            if (type == ResourceType.Gold) {
                reply_error(session, packet, Error.RESOURCE_NOT_TRADABLE);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.getCity(cityId);

                if (city == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (obj != null) {
                    Error ret;
                    ResourceSellAction rsa = new ResourceSellAction(cityId, objectId, price, quantity, type);
                    if (
                        (ret =
                         city.Worker.DoActive(StructureFactory.getActionWorkerType(obj), obj, rsa, obj.Technologies)) ==
                        0)
                        reply_success(session, packet);
                    else
                        reply_error(session, packet, ret);
                    return;
                }
                reply_error(session, packet, Error.UNEXPECTED);
            }
        }
    }
}