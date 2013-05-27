#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class RegionCommandsModule : CommandModule
    {
        private readonly RadiusLocator radiusLocator;

        private readonly IWorld world;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly IRoadPathFinder roadPathFinder;

        private readonly ILocker locker;

        private readonly Util.Channel channel;

        private readonly TileLocator tileLocator;

        public RegionCommandsModule(RadiusLocator radiusLocator,
                                    IWorld world,
                                    ObjectTypeFactory objectTypeFactory,
                                    IRoadPathFinder roadPathFinder,
                                    ILocker locker,
                                    Util.Channel channel,
                                    TileLocator tileLocator)
        {
            this.radiusLocator = radiusLocator;
            this.world = world;
            this.objectTypeFactory = objectTypeFactory;
            this.roadPathFinder = roadPathFinder;
            this.locker = locker;
            this.channel = channel;
            this.tileLocator = tileLocator;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.RegionRoadBuild, CmdRoadCreate);
            processor.RegisterCommand(Command.RegionRoadDestroy, CmdRoadDestroy);
            processor.RegisterCommand(Command.NotificationLocate, CmdNotificationLocate);
            processor.RegisterCommand(Command.CityLocate, CmdCityLocate);
            processor.RegisterCommand(Command.CityLocateByName, CmdCityLocateByName);
            processor.RegisterCommand(Command.RegionGet, CmdGetRegion);
            processor.RegisterCommand(Command.CityRegionGet, CmdGetCityRegion);
        }

        private void CmdRoadCreate(Session session, Packet packet)
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

            if (!world.Regions.IsValidXandY(x, y))
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            // Make sure there is no structure at this point that has no road requirement
            if (world.Regions[x, y].Any(
                                        s =>
                                        s is IStructure &&
                                        objectTypeFactory.IsStructureType("NoRoadRequired", (IStructure)s)))
            {
                ReplyError(session, packet, Error.StructureExists);
                return;
            }

            ICity city;
            using (locker.Lock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                // Make sure user is building road within city walls
                if (tileLocator.TileDistance(city.X, city.Y, x, y) >= city.Radius)
                {
                    ReplyError(session, packet, Error.NotWithinWalls);
                    return;
                }

                world.Regions.LockRegion(x, y);

                // Make sure this tile is not already a road
                if (world.Roads.IsRoad(x, y))
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadAlreadyExists);
                    return;
                }

                // Make sure there is a road next to this tile
                bool hasRoad = false;
                radiusLocator.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            (origX, origY, x1, y1, custom) =>
                                                {
                                                    if (radiusLocator.RadiusDistance(origX, origY, x1, y1) != 1)
                                                    {
                                                        return true;
                                                    }

                                                    if (city.X == x1 && city.Y == y1)
                                                    {
                                                        hasRoad = true;
                                                        return false;
                                                    }

                                                    if (world.Roads.IsRoad(x1, y1) && !world.Regions[x1, y1].Exists(s => s is IStructure))
                                                    {
                                                        hasRoad = true;
                                                        return false;
                                                    }

                                                    return true;
                                                },
                                            null);

                if (!hasRoad)
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadNotAround);
                    return;
                }

                if (!objectTypeFactory.IsTileType("TileBuildable", world.Regions.GetTileType(x, y)))
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.TileMismatch);
                    return;
                }

                world.Roads.CreateRoad(x, y);

                world.Regions.UnlockRegion(x, y);

                session.Write(reply);
            }
        }

        private void CmdRoadDestroy(Session session, Packet packet)
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

            if (!world.Regions.IsValidXandY(x, y))
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ICity city;
            using (locker.Lock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                // Make sure user is building road within city walls
                if (tileLocator.TileDistance(city.X, city.Y, x, y) >= city.Radius)
                {
                    ReplyError(session, packet, Error.NotWithinWalls);
                    return;
                }

                world.Regions.LockRegion(x, y);

                // Make sure this tile is indeed a road
                if (!world.Roads.IsRoad(x, y))
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.TileMismatch);
                    return;
                }

                // Make sure there is no structure at this point
                if (world.Regions[x, y].Exists(s => s is IStructure))
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.StructureExists);
                    return;
                }

                // Make sure there is a road next to this tile
                bool breaksRoad = city
                        .Where(str => !str.IsMainBuilding && !objectTypeFactory.IsStructureType("NoRoadRequired", str))
                        .Any(str => !roadPathFinder.HasPath(new Position(str.X, str.Y), new Position(city.X, city.Y), city, new Position(x, y)));

                if (breaksRoad)
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadDestroyUniquePath);
                    return;
                }

                // Make sure all neighboring roads have a diff path
                bool allNeighborsHaveOtherPaths = true;
                radiusLocator.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                {
                                                    if (radiusLocator.RadiusDistance(origX, origY, x1, y1) != 1)
                                                    {
                                                        return true;
                                                    }

                                                    if (city.X == x1 && city.Y == y1)
                                                    {
                                                        return true;
                                                    }

                                                    if (!world.Roads.IsRoad(x1, y1))
                                                    {
                                                        return true;
                                                    }

                                                    if (roadPathFinder.HasPath(new Position(x1, y1),
                                                                               new Position(city.X, city.Y),
                                                                               city,
                                                                               new Position(origX, origY)))
                                                    {
                                                        return true;
                                                    }

                                                    allNeighborsHaveOtherPaths = false;
                                                    return false;
                                                },
                                            null);

                if (!allNeighborsHaveOtherPaths)
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadDestroyUniquePath);
                    return;
                }

                world.Roads.DestroyRoad(x, y);

                world.Regions.UnlockRegion(x, y);
                session.Write(reply);
            }
        }

        private void CmdCityLocate(Session session, Packet packet)
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

            ICity city;
            using (locker.Lock(cityId, out city))
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

        private void CmdCityLocateByName(Session session, Packet packet)
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
            if (!world.Cities.FindCityId(cityName, out cityId))
            {
                ReplyError(session, packet, Error.CityNotFound);
                return;
            }

            ICity city;
            using (locker.Lock(cityId, out city))
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

        private void CmdNotificationLocate(Session session, Packet packet)
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
            using (locker.Lock(session.Player))
            {
                if (session.Player.GetCity(cityId) == null && session.Player.GetCity(srcCityId) == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }

            Dictionary<uint, ICity> cities;
            using (locker.Lock(out cities, srcCityId, cityId))
            {
                if (cities == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                ICity srcCity = cities[srcCityId];
                ICity city = cities[cityId];

                Logic.Notifications.Notification notification;
                if (!srcCity.Notifications.TryGetValue(city, actionId, out notification))
                {
                    ReplyError(session, packet, Error.ActionNotFound);
                    return;
                }

                reply.AddUInt32(notification.GameObject.X);
                reply.AddUInt32(notification.GameObject.Y);

                session.Write(reply);
            }
        }

        private void CmdGetRegion(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            reply.Option |= (ushort)Packet.Options.Compressed;

            ushort regionId;

            byte regionSubscribeCount;
            try
            {
                regionSubscribeCount = packet.GetByte();

                if (regionSubscribeCount > 15)
                {
                    throw new Exception("Too many regions requested");
                }
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

                var region = world.Regions.GetRegion(regionId);
                if (region == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt16(regionId);
                reply.AddBytes(region.GetObjectBytes());
                world.Regions.SubscribeRegion(session, regionId);
            }

            byte regionUnsubscribeCount;
            try
            {
                regionUnsubscribeCount = packet.GetByte();

                if (regionUnsubscribeCount > 15)
                {
                    throw new Exception("Too many unsubscribe regions");
                }
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

                world.Regions.UnsubscribeRegion(session, regionId);
            }

            if (channel.SubscriptionCount(session) > 30)
            {
                session.CloseSession();
            }
            else
            {
                session.Write(reply);
            }
        }

        private void CmdGetCityRegion(Session session, Packet packet)
        {
            var reply = new Packet(packet);

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
                ushort regionId;
                try
                {
                    regionId = packet.GetUInt16();
                }
                catch(Exception)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var region = world.Regions.CityRegions.GetCityRegion(regionId);
                if (region == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt16(regionId);
                reply.AddBytes(region.GetCityBytes());
            }

            session.Write(reply);
        }
    }
}