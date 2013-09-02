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
        private readonly ITileLocator tileLocator;

        private readonly IWorld world;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly IRoadPathFinder roadPathFinder;

        private readonly ILocker locker;

        private readonly Util.IChannel channel;

        public RegionCommandsModule(ITileLocator tileLocator,
                                    IWorld world,
                                    IObjectTypeFactory objectTypeFactory,
                                    IRoadPathFinder roadPathFinder,
                                    ILocker locker,
                                    Util.IChannel channel)
        {
            this.tileLocator = tileLocator;
            this.world = world;
            this.objectTypeFactory = objectTypeFactory;
            this.roadPathFinder = roadPathFinder;
            this.locker = locker;
            this.channel = channel;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.RegionRoadBuild, RoadCreate);
            processor.RegisterCommand(Command.RegionRoadDestroy, RoadDestroy);
            processor.RegisterCommand(Command.NotificationLocate, NotificationLocate);
            processor.RegisterCommand(Command.CityLocate, CityLocate);
            processor.RegisterCommand(Command.CityLocateByName, CityLocateByName);
            processor.RegisterCommand(Command.RegionGet, GetRegion);
            processor.RegisterCommand(Command.MiniMapRegionGet, GetMiniMapRegion);
        }

        private void RoadCreate(Session session, Packet packet)
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
            if (world.Regions.GetObjectsInTile(x, y).Any(
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
                if (tileLocator.TileDistance(city.PrimaryPosition, 1, new Position(x, y), 1) >= city.Radius)
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

                foreach (var position in tileLocator.ForeachRadius(x, y, 1, false))
                {
                    if ((world.Roads.IsRoad(position.X, position.Y) && 
                        !world.Regions.GetObjectsInTile(position.X, position.Y).Any(s => s is IStructure && s != city.MainBuilding)))
                    {
                        hasRoad = true;
                        break;
                    }
                }

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

        private void RoadDestroy(Session session, Packet packet)
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
                if (tileLocator.TileDistance(city.PrimaryPosition, 1, new Position(x, y), 1) >= city.Radius)
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
                if (world.Regions.GetObjectsInTile(x, y).Any(s => s is IStructure))
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.StructureExists);
                    return;
                }

                // Make sure there is a road next to this tile
                bool breaksRoad = city
                        .Where(str => !str.IsMainBuilding && !objectTypeFactory.IsStructureType("NoRoadRequired", str))
                        .Any(str => !roadPathFinder.HasPath(start: new Position(str.PrimaryPosition.X, str.PrimaryPosition.Y),
                                                            end: new Position(city.PrimaryPosition.X, city.PrimaryPosition.Y),
                                                            city: city,
                                                            excludedPoint: new Position(x, y)));

                if (breaksRoad)
                {
                    world.Regions.UnlockRegion(x, y);
                    ReplyError(session, packet, Error.RoadDestroyUniquePath);
                    return;
                }

                // Make sure all neighboring roads have a diff path
                bool allNeighborsHaveOtherPaths = true;
                foreach (var position in tileLocator.ForeachRadius(x, y, 1, false))
                {
                    if (tileLocator.RadiusDistance(new Position(x, y), 1, position, 1) != 1)
                    {
                        continue;
                    }

                    if (city.PrimaryPosition.X == position.X && city.PrimaryPosition.Y == position.Y)
                    {
                        continue;
                    }

                    if (!world.Roads.IsRoad(position.X, position.Y))
                    {
                        continue;
                    }

                    if (roadPathFinder.HasPath(new Position(position.X, position.Y),
                                               new Position(city.PrimaryPosition.X, city.PrimaryPosition.Y),
                                               city,
                                               new Position(x, y)))
                    {
                        continue;
                    }

                    allNeighborsHaveOtherPaths = false;
                    break;
                }

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

        private void CityLocate(Session session, Packet packet)
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

                reply.AddUInt32(city.PrimaryPosition.X);
                reply.AddUInt32(city.PrimaryPosition.Y);

                session.Write(reply);
            }
        }

        private void CityLocateByName(Session session, Packet packet)
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

                reply.AddUInt32(city.PrimaryPosition.X);
                reply.AddUInt32(city.PrimaryPosition.Y);

                session.Write(reply);
            }
        }

        private void NotificationLocate(Session session, Packet packet)
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

                reply.AddUInt32(notification.GameObject.PrimaryPosition.X);
                reply.AddUInt32(notification.GameObject.PrimaryPosition.Y);

                session.Write(reply);
            }
        }

        private void GetRegion(Session session, Packet packet)
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

        private void GetMiniMapRegion(Session session, Packet packet)
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

                MiniMapRegion region;
                if (!world.Regions.MiniMapRegions.TryGetMiniMapRegion(regionId, out region))
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