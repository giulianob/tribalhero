using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Comm;
using Game.Data;
using Game.Data.Events;
using Game.Setup;
using Game.Util;
using Ninject.Extensions.Logging;

namespace Game.Map
{
    public class RegionManager : IRegionManager
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public event EventHandler<ObjectEvent> ObjectAdded;

        private readonly IMiniMapRegionManagerFactory miniMapRegionManagerFactory;

        private readonly IRegionFactory regionFactory;

        private readonly ITileLocator tileLocator;

        private readonly IChannel channel;

        private readonly IRegionLocator regionLocator;

        private List<IRegion> regions = new List<IRegion>();

        public RegionManager(IMiniMapRegionManagerFactory miniMapRegionManagerFactory,
                             IRegionFactory regionFactory,
                             ITileLocator tileLocator,
                             IChannel channel,
                             IRegionLocator regionLocator)
        {
            this.miniMapRegionManagerFactory = miniMapRegionManagerFactory;
            this.regionFactory = regionFactory;
            this.tileLocator = tileLocator;
            this.channel = channel;
            this.regionLocator = regionLocator;
        }

        private int RegionsCount { get; set; }

        private uint RegionSize { get; set; }

        private Stream RegionChanges { get; set; }

        private byte[] MapData { get; set; }

        public IMiniMapRegionManager MiniMapRegions { get; private set; }

        private uint WorldWidth { get; set; }

        private uint WorldHeight { get; set; }

        public bool IsValidXandY(uint x, uint y)
        {
            return x < Config.map_width && y < Config.map_height;
        }

        public IEnumerable<ushort> GetRegionIds(uint x, uint y, byte size)
        {
            return tileLocator.ForeachMultitile(x, y, size)
                              .Select(p => regionLocator.GetRegionIndex(p.X, p.Y))
                              .Distinct()
                              .OrderBy(p => p);
        }

        public IEnumerable<ushort> LockRegions(uint x, uint y, byte size)
        {
            return LockRegions(GetRegionIds(x, y, size));
        }

        private IEnumerable<ushort> LockRegions(IEnumerable<ushort> regionIds)
        {
            var lockedRegions = new List<ushort>();
            foreach (var regionId in regionIds)
            {
                var region = GetRegion(regionId);

                if (region != null)
                {
                    region.EnterWriteLock();
                    lockedRegions.Add(regionId);
                }
            }

            return lockedRegions;
        }

        public void UnlockRegions(uint x, uint y, byte size)
        {
            UnlockRegions(tileLocator.ForeachMultitile(x, y, size)
                                     .Select(p => regionLocator.GetRegionIndex(p.X, p.Y))
                                     .Distinct());
        }

        public void UnlockRegions(IEnumerable<ushort> regionIds)
        {
            foreach (var regionId in regionIds.OrderByDescending(p => p))
            {
                var region = GetRegion(regionId);

                if (region != null)
                {
                    region.ExitWriteLock();
                }
            }
        }

        public void LockRegion(uint x, uint y)
        {
            GetRegion(x, y).EnterWriteLock();
        }

        public void UnlockRegion(uint x, uint y)
        {
            GetRegion(x, y).ExitWriteLock();
        }

        public IEnumerable<ISimpleGameObject> GetObjectsWithin(uint x, uint y, int radius)
        {
            var list = new List<ISimpleGameObject>();
            foreach (var position in tileLocator.ForeachTile(x, y, radius, false))
            {
                if (position.X < WorldWidth && position.Y < WorldHeight)
                {
                    list.AddRange(GetObjectsInTile(position.X, position.Y));
                }
            }

            return list;
        }

        public IEnumerable<ushort> GetTilesWithin(uint x, uint y, byte radius)
        {
            return from position in tileLocator.ForeachTile(x, y, radius, false)
                   where position.X < WorldWidth && position.Y < WorldHeight
                   select GetTileType(position.X, position.Y);
        }

        public bool Add(ISimpleGameObject obj)
        {
            if (!AddToPrimaryRegionAndTiles(obj))
            {
                return false;
            }

            // Keeps track of objects that exist in the map
            obj.InWorld = true;

            RegisterObjectEventListeners(obj);

            // Add appropriate objects to the minimap
            IMiniMapRegionObject miniMapRegionObject = obj as IMiniMapRegionObject;
            if (miniMapRegionObject != null)
            {
                MiniMapRegions.Add(miniMapRegionObject);
            }

            // Post to channel
            ushort regionId = regionLocator.GetRegionIndex(obj);

            channel.Post("/WORLD/" + regionId, () =>
                {
                    var packet = new Packet(Command.ObjectAdd);
                    packet.AddUInt16(regionId);
                    PacketHelper.AddToPacket(obj, packet);
                    return packet;
                });

            // Raise event
            ObjectAdded.Raise(this, new ObjectEvent(obj));

            return true;
        }

        private bool AddToPrimaryRegionAndTiles(ISimpleGameObject obj)
        {
            var primaryRegion = GetRegion(obj.PrimaryPosition.X, obj.PrimaryPosition.Y);

            if (primaryRegion == null)
            {
                return false;
            }

            primaryRegion.Add(obj);

            foreach (var position in tileLocator.ForeachMultitile(obj))
            {
                var region = GetRegion(position.X, position.Y);

                if (region != null)
                {
                    region.AddObjectToTile(obj, position.X, position.Y);
                }
            }

            return true;
        }

        private void RegisterObjectEventListeners(ISimpleGameObject simpleGameObject)
        {
            simpleGameObject.ObjectUpdated += SimpleGameObjectOnObjectUpdated;
        }

        private void DeregisterObjectEventListeners(ISimpleGameObject simpleGameObject)
        {
            simpleGameObject.ObjectUpdated -= SimpleGameObjectOnObjectUpdated;
        }

        private void SimpleGameObjectOnObjectUpdated(object sender, SimpleGameObjectArgs e)
        {
            ObjectUpdateEvent(e.SimpleGameObject, e.OriginalX, e.OriginalY);
        }

        public void DbLoaderAdd(ISimpleGameObject obj)
        {
            if (obj.InWorld)
            {
                Add(obj);
            }
        }

        public void Remove(ISimpleGameObject obj)
        {
            if (!obj.InWorld)
            {
                return;
            }

            var lockedRegions = LockRegions(obj.PrimaryPosition.X, obj.PrimaryPosition.Y, obj.Size);

            if (RemoveFromPrimaryRegionAndAllTiles(obj, obj.PrimaryPosition.X, obj.PrimaryPosition.Y))
            {
                obj.InWorld = false;

                DeregisterObjectEventListeners(obj);

                IMiniMapRegionObject miniMapRegionObject = obj as IMiniMapRegionObject;
                if (miniMapRegionObject != null)
                {
                    MiniMapRegions.Remove(miniMapRegionObject);
                }

                ushort regionId = regionLocator.GetRegionIndex(obj);
                channel.Post("/WORLD/" + regionId, () =>
                    {
                        var packet = new Packet(Command.ObjectRemove);
                        packet.AddUInt16(regionId);
                        packet.AddUInt32(obj.GroupId);
                        packet.AddUInt32(obj.ObjectId);
                        return packet;
                    });
            }
            
            UnlockRegions(lockedRegions);
        }

        private bool RemoveFromPrimaryRegionAndAllTiles(ISimpleGameObject obj, uint x, uint y)
        {
            var primaryRegion = GetRegion(x, y);

            if (primaryRegion == null)
            {
                return false;
            }

            primaryRegion.Remove(obj, x, y);

            foreach (var position in tileLocator.ForeachMultitile(x, y, obj.Size))
            {
                var region = GetRegion(position.X, position.Y);
                if (region != null)
                {
                    region.RemoveObjectFromTile(obj, position.X, position.Y);
                }
            }

            return true;
        }

        public IEnumerable<ISimpleGameObject> GetObjectsInTile(uint x, uint y)
        {
            IRegion region = GetRegion(x, y);
            return region == null ? new ISimpleGameObject[0] : region.GetObjectsInTile(x, y);
        }

        public IRegion GetRegion(uint x, uint y)
        {
            return GetRegion(regionLocator.GetRegionIndex(x, y));
        }

        public IRegion GetRegion(ushort id)
        {
            return id >= regions.Count ? null : regions[id];
        }

        public void ObjectUpdateEvent(ISimpleGameObject sender, uint origX, uint origY)
        {
            if (!sender.InWorld)
            {
                throw new Exception(string.Format("Received update for obj that is not in world: eventOrigX[{1}] eventOrigY[{2}] {0}",
                                                  sender.ToString(),
                                                  origX,
                                                  origY));
            }

            var miniMapRegionObject = sender as IMiniMapRegionObject;
            if (miniMapRegionObject != null)
            {
                MiniMapRegions.UpdateObjectRegion(miniMapRegionObject, origX, origY);
            }

            // Lock regions from both old and new positions
            var lockedRegions = LockRegions(GetRegionIds(sender.PrimaryPosition.X, sender.PrimaryPosition.Y, sender.Size)
                                                    .Concat(GetRegionIds(origX, origY, sender.Size)));

            ushort previousPrimaryRegionId = regionLocator.GetRegionIndex(origX, origY);
            ushort newPrimaryRegionId = regionLocator.GetRegionIndex(sender);

            RemoveFromPrimaryRegionAndAllTiles(sender, origX, origY);
            AddToPrimaryRegionAndTiles(sender);

            if (previousPrimaryRegionId == newPrimaryRegionId)
            {
                var packet = new Packet(Command.ObjectUpdate);
                packet.AddUInt16(newPrimaryRegionId);
                PacketHelper.AddToPacket(sender, packet);
                channel.Post("/WORLD/" + newPrimaryRegionId, packet);
            }
            else
            {
                var packet = new Packet(Command.ObjectMove);
                packet.AddUInt16(previousPrimaryRegionId);
                packet.AddUInt16(newPrimaryRegionId);
                PacketHelper.AddToPacket(sender, packet);
                channel.Post("/WORLD/" + previousPrimaryRegionId, packet);

                packet = new Packet(Command.ObjectAdd);
                packet.AddUInt16(newPrimaryRegionId);
                PacketHelper.AddToPacket(sender, packet);
                channel.Post("/WORLD/" + newPrimaryRegionId, packet);
            }

            UnlockRegions(lockedRegions);
        }

        public void AddRegion(IRegion region)
        {
            regions.Add(region);
        }

        public void Load(Stream mapStream,
                         Stream regionChangesStream,
                         bool createRegionChanges,
                         uint inWorldWidth,
                         uint inWorldHeight,
                         uint regionWidth,
                         uint regionHeight,
                         uint miniMapRegionWidth,
                         uint miniMapRegionHeight)
        {
            logger.Info("Loading map...");

            if (mapStream == null)
            {
                throw new Exception("Missing map");
            }

            if (regionChangesStream == null)
            {
                throw new Exception("Missing region changes");
            }

            if (regionWidth == 0 || regionHeight == 0 || inWorldWidth % regionWidth != 0 ||
                inWorldHeight % regionHeight != 0)
            {
                throw new Exception("Invalid region size configured");
            }

            if (miniMapRegionWidth == 0 || miniMapRegionHeight == 0 || inWorldWidth % miniMapRegionWidth != 0 ||
                inWorldHeight % miniMapRegionHeight != 0)
            {
                throw new Exception("Invalid city region size configured");
            }

            WorldWidth = inWorldWidth;
            WorldHeight = inWorldHeight;

            // creating regions;                    
            RegionSize = regionWidth * regionHeight;

            var column = (int)(inWorldWidth / regionWidth);
            var row = (int)(inWorldHeight / regionHeight);

            RegionsCount = column * row;
            MapData = new byte[RegionSize * Region.TILE_SIZE * RegionsCount];

            RegionChanges = regionChangesStream;

            regions = new List<IRegion>(RegionsCount);
            for (int regionId = 0; regionId < RegionsCount; ++regionId)
            {
                var data = new byte[RegionSize * Region.TILE_SIZE]; // 1 tile is 2 bytes

                var mapDataOffset = (int)(RegionSize * Region.TILE_SIZE * regionId);

                if (mapStream.Read(MapData, mapDataOffset, (int)(RegionSize * Region.TILE_SIZE)) !=
                    RegionSize * Region.TILE_SIZE)
                {
                    throw new Exception("Not enough map data");
                }

                if (createRegionChanges)
                {
                    Buffer.BlockCopy(MapData, mapDataOffset, data, 0, data.Length);
                    regionChangesStream.Write(data, 0, data.Length);
                }
                else
                {
                    if (RegionChanges.Read(data, 0, (int)(RegionSize * Region.TILE_SIZE)) !=
                        RegionSize * Region.TILE_SIZE)
                    {
                        throw new Exception("Not enough region change map data");
                    }
                }

                AddRegion(regionFactory.CreateRegion(data));
            }

            logger.Info(String.Format("Loaded map file length[{0}] position[{1}] regions[{2}]", mapStream.Length, mapStream.Position, regions.Count));            

            // creating city regions;
            column = (int)(inWorldWidth / miniMapRegionWidth);
            row = (int)(inWorldHeight / miniMapRegionHeight);
            int miniMapRegionsCount = column * row;

            MiniMapRegions = miniMapRegionManagerFactory.CreateMiniMapRegionManager(miniMapRegionsCount);
        }

        public void Unload()
        {
            RegionChanges.Close();
        }

        #region Tile Methods

        public ushort GetTileType(uint x, uint y)
        {
            IRegion region = GetRegion(x, y);
            return region.GetTileType(x, y);
        }

        public ushort RevertTileType(uint x, uint y, bool sendEvent)
        {
            ushort regionId = regionLocator.GetRegionIndex(x, y);
            ushort tileType;

            lock (RegionChanges)
            {
                IRegion region = GetRegion(x, y);

                long idx = (regionLocator.GetTileIndex(x, y) * Region.TILE_SIZE) + (Region.TILE_SIZE * RegionSize * regionId);
                tileType = MapData[idx];

                // Check if it's actually changed
                if (region.GetTileType(x, y) == tileType)
                {
                    return tileType;
                }

                RegionChanges.Seek(idx, SeekOrigin.Begin);
                RegionChanges.Write(BitConverter.GetBytes(tileType), 0, 2);
                RegionChanges.Flush();

                region.SetTileType(x, y, tileType);
            }

            if (sendEvent && Global.Current.FireEvents)
            {
                var packet = new Packet(Command.RegionSetTile);
                packet.AddUInt16(1);
                packet.AddUInt32(x);
                packet.AddUInt32(y);
                packet.AddUInt16(tileType);

                channel.Post("/WORLD/" + regionId, packet);
            }

            return tileType;
        }

        public void SetTileType(uint x, uint y, ushort tileType, bool sendEvent)
        {
            ushort regionId = regionLocator.GetRegionIndex(x, y);

            lock (RegionChanges)
            {
                IRegion region = GetRegion(x, y);

                long idx = (regionLocator.GetTileIndex(x, y) * Region.TILE_SIZE) + (Region.TILE_SIZE * RegionSize * regionId);
                RegionChanges.Seek(idx, SeekOrigin.Begin);
                RegionChanges.Write(BitConverter.GetBytes(tileType), 0, 2);
                RegionChanges.Flush();

                region.SetTileType(x, y, tileType);
            }

            if (sendEvent && Global.Current.FireEvents)
            {
                var packet = new Packet(Command.RegionSetTile);
                packet.AddUInt16(1);
                packet.AddUInt32(x);
                packet.AddUInt32(y);
                packet.AddUInt16(tileType);

                channel.Post("/WORLD/" + regionId, packet);
            }
        }

        #endregion

        #region Channel Subscriptions

        public void SubscribeRegion(Session session, ushort id)
        {
            try
            {
                channel.Subscribe(session, "/WORLD/" + id);
            }
            catch(DuplicateSubscriptionException)
            {
            }
        }

        public void UnsubscribeRegion(Session session, ushort id)
        {
            channel.Unsubscribe(session, "/WORLD/" + id);
        }

        #endregion
    }
}