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
                srcCityId = packet.GetUInt32();
                cityId = packet.GetUInt32();
                actionId = packet.GetUInt16();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            //check to make sure that the city belongs to us
            using (new MultiObjectLock(session.Player)) {
                if (session.Player.getCity(cityId) == null && session.Player.getCity(srcCityId) == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, srcCityId, cityId)) {
                if (cities == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                City srcCity = cities[srcCityId];
                City city = cities[cityId];

                NotificationManager.Notification notification;
                if (!srcCity.Worker.Notifications.tryGetValue(city, actionId, out notification)) {
                    ReplyError(session, packet, Error.ACTION_NOT_FOUND);
                    return;
                }

                reply.AddUInt32(notification.GameObject.X);
                reply.AddUInt32(notification.GameObject.Y);

                session.write(reply);
            }
        }

        public void CmdGetRegion(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            ushort regionId;

            byte regionSubscribeCount;
            try {
                regionSubscribeCount = packet.GetByte();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.AddByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i) {
                try {
                    regionId = packet.GetUInt16();
                }
                catch (Exception) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (regionId >= Config.regions_count) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.AddUInt16(regionId);
                reply.AddBytes(Global.World.GetRegion(regionId).getBytes());
                reply.AddBytes(Global.World.GetRegion(regionId).getObjectBytes());
                Global.World.SubscribeRegion(session, regionId);
            }

            byte regionUnsubscribeCount;
            try {
                regionUnsubscribeCount = packet.GetByte();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            for (uint i = 0; i < regionUnsubscribeCount; ++i) {
                try {
                    regionId = packet.GetUInt16();
                }
                catch (Exception) {
                    ReplyError(session, packet, Error.UNEXPECTED);
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
                regionSubscribeCount = packet.GetByte();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.AddByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i) {
                try {
                    regionId = packet.GetUInt16();
                }
                catch (Exception) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (regionId >= Config.regions_count) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.AddUInt16(regionId);
                reply.AddBytes(Global.World.GetCityRegion(regionId).GetCityBytes());
            }

            session.write(reply);
        }        
    }
}