#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm {
    public partial class Processor {
        public void CmdNotificationLocate(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            uint srcCityId;
            uint cityId;
            ushort actionId;

            try {
                srcCityId = packet.getUInt32();
                cityId = packet.getUInt32();
                actionId = packet.getUInt16();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            //check to make sure that the city belongs to us
            using (new MultiObjectLock(session.Player)) {
                if (session.Player.getCity(cityId) == null && session.Player.getCity(srcCityId) == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, srcCityId, cityId)) {
                if (cities == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                City srcCity = cities[srcCityId];
                City city = cities[cityId];

                NotificationManager.Notification notification;
                if (!srcCity.Worker.Notifications.tryGetValue(city, actionId, out notification)) {
                    reply_error(session, packet, Error.ACTION_NOT_FOUND);
                    return;
                }

                reply.addUInt32(notification.GameObject.X);
                reply.addUInt32(notification.GameObject.Y);

                session.write(reply);
            }
        }

        public void CmdGetRegion(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            ushort regionId;

            byte regionSubscribeCount;
            try {
                regionSubscribeCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.addByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i) {
                try {
                    regionId = packet.getUInt16();
                }
                catch (Exception) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (regionId >= Config.regions_count) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.addUInt16(regionId);
                reply.addBytes(Global.World.GetRegion(regionId).getBytes());
                reply.addBytes(Global.World.GetRegion(regionId).getObjectBytes());
                Global.World.SubscribeRegion(session, regionId);
            }

            byte regionUnsubscribeCount;
            try {
                regionUnsubscribeCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            for (uint i = 0; i < regionUnsubscribeCount; ++i) {
                try {
                    regionId = packet.getUInt16();
                }
                catch (Exception) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                Global.World.UnsubscribeRegion(session, regionId);
            }

            session.write(reply);
        }

        public void CmdGetCityRegion(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            ushort regionId;
            
            byte regionSubscribeCount;
            try {
                regionSubscribeCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.addByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i) {
                try {
                    regionId = packet.getUInt16();
                }
                catch (Exception) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (regionId >= Config.regions_count) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.addUInt16(regionId);
                reply.addBytes(Global.World.GetCityRegion(regionId).getCityBytes());
            }

            session.write(reply);
        }        
    }
}