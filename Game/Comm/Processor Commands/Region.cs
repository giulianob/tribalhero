#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Logic;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdRoadCreate(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            uint x;
            uint y;
            uint cityId;

            try
            {
                cityId = packet.GetUInt32();
                x = packet.GetUInt32();
                y = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (!Global.World.IsValidXandY(x, y))
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            // Make sure there is no structure at this point that has no road requirement
            if (Global.World[x, y].Any(s => s is Structure && ObjectTypeFactory.IsStructureType("NoRoadRequired", (Structure)s)))
            {
                ReplyError(session, packet, Error.StructureExists);
                return;
            }

            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                // Make sure user is building road within city walls
                if (SimpleGameObject.TileDistance(city.X, city.Y, x, y) >= city.Radius)
                {
                    ReplyError(session, packet, Error.NotWithinWalls);
                    return;
                }

                Global.World.LockRegion(x, y);

                // Make sure this tile is not already a road
                if (RoadManager.IsRoad(x, y))
                {
                    Global.World.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadAlreadyExists);
                    return;
                }

                // Make sure there is a road next to this tile
                bool hasRoad = false;
                RadiusLocator.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                {
                                                    if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                        return true;

                                                    if (city.X == x1 && city.Y == y1)
                                                    {
                                                        hasRoad = true;
                                                        return false;
                                                    }

                                                    if (RoadManager.IsRoad(x1, y1) && !Global.World[x1, y1].Exists(s => s is Structure))
                                                    {
                                                        hasRoad = true;
                                                        return false;
                                                    }

                                                    return true;
                                                },
                                            null);

                if (!hasRoad)
                {
                    Global.World.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadNotAround);
                    return;
                }

                if (!ObjectTypeFactory.IsTileType("TileBuildable", Global.World.GetTileType(x, y)))
                {
                    Global.World.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.TileMismatch);
                    return;
                }

                Global.World.RoadManager.CreateRoad(x, y);

                Global.World.UnlockRegion(x, y);

                session.Write(reply);
            }
        }

        public void CmdRoadDestroy(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            uint x;
            uint y;
            uint cityId;

            try
            {
                cityId = packet.GetUInt32();
                x = packet.GetUInt32();
                y = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (!Global.World.IsValidXandY(x, y))
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                // Make sure user is building road within city walls
                if (SimpleGameObject.TileDistance(city.X, city.Y, x, y) >= city.Radius)
                {
                    ReplyError(session, packet, Error.NotWithinWalls);
                    return;
                }

                Global.World.LockRegion(x, y);

                // Make sure this tile is indeed a road
                if (!RoadManager.IsRoad(x, y))
                {
                    Global.World.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.TileMismatch);
                    return;
                }

                // Make sure there is no structure at this point
                if (Global.World[x, y].Exists(s => s is Structure))
                {
                    Global.World.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.StructureExists);
                    return;
                }

                // Make sure there is a road next to this tile
                bool breaksRoad = false;

                foreach (var str in city)
                {
                    if (str.IsMainBuilding || ObjectTypeFactory.IsStructureType("NoRoadRequired", str))
                        continue;

                    if (!RoadPathFinder.HasPath(new Location(str.X, str.Y), new Location(city.X, city.Y), city, new Location(x, y)))
                    {
                        breaksRoad = true;
                        break;
                    }
                }

                if (breaksRoad)
                {
                    Global.World.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadDestroyUniquePath);
                    return;
                }

                // Make sure all neighboring roads have a diff path
                bool allNeighborsHaveOtherPaths = true;
                RadiusLocator.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                {
                                                    if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                        return true;

                                                    if (city.X == x1 && city.Y == y1)
                                                        return true;

                                                    if (RoadManager.IsRoad(x1, y1))
                                                    {
                                                        if (
                                                                !RoadPathFinder.HasPath(new Location(x1, y1),
                                                                                        new Location(city.X, city.Y),
                                                                                        city,
                                                                                        new Location(origX, origY)))
                                                        {
                                                            allNeighborsHaveOtherPaths = false;
                                                            return false;
                                                        }
                                                    }

                                                    return true;
                                                },
                                            null);

                if (!allNeighborsHaveOtherPaths)
                {
                    Global.World.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadDestroyUniquePath);
                    return;
                }

                Global.World.RoadManager.DestroyRoad(x, y);

                Global.World.UnlockRegion(x, y);
                session.Write(reply);
            }
        }

        public void CmdCityLocate(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            uint cityId;

            try
            {
                cityId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                reply.AddUInt32(city.X);
                reply.AddUInt32(city.Y);

                session.Write(reply);
            }
        }

        public void CmdCityLocateByName(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            string cityName;

            try
            {
                cityName = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            uint cityId;
            if (!Global.World.FindCityId(cityName, out cityId))
            {
                ReplyError(session, packet, Error.CityNotFound);
                return;
            }

            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                reply.AddUInt32(city.X);
                reply.AddUInt32(city.Y);

                session.Write(reply);
            }
        }

        public void CmdNotificationLocate(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            uint srcCityId;
            uint cityId;
            ushort actionId;

            try
            {
                srcCityId = packet.GetUInt32();
                cityId = packet.GetUInt32();
                actionId = packet.GetUInt16();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            //check to make sure that the city belongs to us
            using (new MultiObjectLock(session.Player))
            {
                if (session.Player.GetCity(cityId) == null && session.Player.GetCity(srcCityId) == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, srcCityId, cityId))
            {
                if (cities == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                City srcCity = cities[srcCityId];
                City city = cities[cityId];

                NotificationManager.Notification notification;
                if (!srcCity.Worker.Notifications.TryGetValue(city, actionId, out notification))
                {
                    ReplyError(session, packet, Error.ActionNotFound);
                    return;
                }

                reply.AddUInt32(notification.GameObject.X);
                reply.AddUInt32(notification.GameObject.Y);

                session.Write(reply);
            }
        }

        public void CmdGetRegion(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            reply.Option |= (ushort)Packet.Options.Compressed;

            ushort regionId;

            byte regionSubscribeCount;
            try
            {
                regionSubscribeCount = packet.GetByte();

                if (regionSubscribeCount > 15)
                    throw new Exception("Too many regions requested");
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i)
            {
                try
                {
                    regionId = packet.GetUInt16();
                }
                catch(Exception)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (regionId >= Config.regions_count)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt16(regionId);
                reply.AddBytes(Global.World.GetRegion(regionId).GetBytes());
                reply.AddBytes(Global.World.GetRegion(regionId).GetObjectBytes());
                Global.World.SubscribeRegion(session, regionId);
            }

            byte regionUnsubscribeCount;
            try
            {
                regionUnsubscribeCount = packet.GetByte();

                if (regionUnsubscribeCount > 15)
                    throw new Exception("Too many unsubscribe regions");
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            for (uint i = 0; i < regionUnsubscribeCount; ++i)
            {
                try
                {
                    regionId = packet.GetUInt16();
                }
                catch(Exception)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                Global.World.UnsubscribeRegion(session, regionId);
            }

            if (Global.Channel.SubscriptionCount(session) > 30)
                session.CloseSession();
            else
                session.Write(reply);
        }

        public void CmdGetCityRegion(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            ushort regionId;

            byte regionSubscribeCount;
            try
            {
                regionSubscribeCount = packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i)
            {
                try
                {
                    regionId = packet.GetUInt16();
                }
                catch(Exception)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (regionId >= Config.regions_count)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt16(regionId);
                reply.AddBytes(Global.World.GetCityRegion(regionId).GetCityBytes());
            }

            session.Write(reply);
        }
    }
}