using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly ICityRegionManagerFactory cityRegionManagerFactory;

        private readonly IRegionFactory regionFactory;

        private readonly ITileLocator tileLocator;

        private readonly IChannel channel;

        private Region[] regions;

        public RegionManager(ICityRegionManagerFactory cityRegionManagerFactory,
                             IRegionFactory regionFactory,
                             ITileLocator tileLocator,
                             IChannel channel)
        {
            this.cityRegionManagerFactory = cityRegionManagerFactory;
            this.regionFactory = regionFactory;
            this.tileLocator = tileLocator;
            this.channel = channel;
        }

        private int RegionsCount { get; set; }

        private uint RegionSize { get; set; }

        private Stream RegionChanges { get; set; }

        private byte[] MapData { get; set; }        

        public ICityRegionManager CityRegions { get; private set; }

        private uint WorldWidth { get; set; }

        private uint WorldHeight { get; set; }

        public List<ISimpleGameObject> this[uint x, uint y]
        {
            get
            {
                Region region = GetRegion(x, y);
                return region == null ? new List<ISimpleGameObject>() : region.GetObjects(x, y);
            }
        }

        public bool IsValidXandY(uint x, uint y)
        {
            return x < Config.map_width && y < Config.map_height;
        }

        public void LockRegion(uint x, uint y)
        {
            GetRegion(x, y).Lock.EnterWriteLock();
        }

        public void UnlockRegion(uint x, uint y)
        {
            GetRegion(x, y).Lock.ExitWriteLock();
        }

        public List<ISimpleGameObject> GetObjectsWithin(uint x, uint y, int radius)
        {
            var list = new List<ISimpleGameObject>();
            foreach (var position in tileLocator.ForeachTile(x, y, radius, false))
            {
                if (position.X < WorldWidth && position.Y < WorldHeight)
                {
                    list.AddRange(GetObjects(position.X, position.Y));
                }
            }

            return list;
        }

        public List<ushort> GetTilesWithin(uint x, uint y, byte radius)
        {
            var list = new List<ushort>();
            foreach (var position in tileLocator.ForeachTile(x, y, radius, false))
            {
                if (position.X < WorldWidth && position.Y < WorldHeight)
                {
                    list.Add(GetTileType(position.X, position.Y));
                }
            }

            return list;
        }

        public bool Add(ISimpleGameObject obj)
        {
            var region = GetRegion(obj.X, obj.Y);

            if (region == null)
            {
                return false;
            }

            region.Add(obj);

            // Keeps track of objects that exist in the map
            obj.InWorld = true;

            RegisterObjectEventListeners(obj);

            // Add appropriate objects to the minimap
            ICityRegionObject cityRegionObject = obj as ICityRegionObject;
            if (cityRegionObject != null)
            {
                CityRegions.Add(cityRegionObject);
            }

            // Post to channel
            ushort regionId = Region.GetRegionIndex(obj);

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
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
            {
                return;
            }

            if (obj.InWorld)
            {
                RegisterObjectEventListeners(obj);

                region.Add(obj);

                // Add to minimap if needed
                ICityRegionObject cityRegionObject = obj as ICityRegionObject;
                if (cityRegionObject != null)
                {
                    CityRegions.Add(cityRegionObject);
                }
            }
        }

        public void Remove(ISimpleGameObject obj)
        {
            Remove(obj, obj.X, obj.Y);
        }

        public List<ISimpleGameObject> GetObjects(uint x, uint y)
        {
            Region region = GetRegion(x, y);
            return region == null ? new List<ISimpleGameObject>() : region.GetObjects(x, y);
        }

        public Region GetRegion(uint x, uint y)
        {
            if (x == 0 && y == 0)
            {
                return null;
            }

            return GetRegion(Region.GetRegionIndex(x, y));
        }

        public Region GetRegion(ushort id)
        {
            if (id >= regions.Length)
            {
                return null;
            }
            return regions[id];
        }

        public void ObjectUpdateEvent(ISimpleGameObject sender, uint origX, uint origY)
        {
            // If object is a city region object, we need to update that
            var cityRegionObject = sender as ICityRegionObject;
            if (cityRegionObject != null)
            {
                CityRegions.UpdateObjectRegion(cityRegionObject, origX, origY);
            }

            //if object has moved then we need to do some logic to see if it has changed regions
            ushort oldRegionId = Region.GetRegionIndex(origX, origY);
            ushort newRegionId = Region.GetRegionIndex(sender);

            if (oldRegionId == newRegionId)
            {
                //object has not changed regions so simply update
                regions[newRegionId].Update(sender, origX, origY);
                var packet = new Packet(Command.ObjectUpdate);
                packet.AddUInt16(newRegionId);
                PacketHelper.AddToPacket(sender, packet);
                channel.Post("/WORLD/" + newRegionId, packet);
            }
            else
            {
                // object has changed regions, need to remove it from the old one and add it to the new one
                regions[oldRegionId].Remove(sender, origX, origY);
                regions[newRegionId].Add(sender);
                var packet = new Packet(Command.ObjectMove);
                packet.AddUInt16(oldRegionId);
                packet.AddUInt16(newRegionId);
                PacketHelper.AddToPacket(sender, packet);
                channel.Post("/WORLD/" + oldRegionId, packet);

                packet = new Packet(Command.ObjectAdd);
                packet.AddUInt16(newRegionId);
                PacketHelper.AddToPacket(sender, packet);
                channel.Post("/WORLD/" + newRegionId, packet);
            }
        }

        public void Load(Stream mapStream,
                         Stream regionChangesStream,
                         bool createRegionChanges,
                         uint inWorldWidth,
                         uint inWorldHeight,
                         uint regionWidth,
                         uint regionHeight,
                         uint cityRegionWidth,
                         uint cityRegionHeight)
        {
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

            if (cityRegionWidth == 0 || cityRegionHeight == 0 || inWorldWidth % cityRegionWidth != 0 ||
                inWorldHeight % cityRegionHeight != 0)
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

            regions = new Region[RegionsCount];
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

                regions[regionId] = regionFactory.CreateRegion(data);
            }

            logger.Info(String.Format("map file length[{0}] position[{1}]", mapStream.Length, mapStream.Position));
            logger.Info(regions.Length + " created.");

            // creating city regions;
            column = (int)(inWorldWidth / cityRegionWidth);
            row = (int)(inWorldHeight / cityRegionHeight);
            int cityRegionsCount = column * row;

            CityRegions = cityRegionManagerFactory.CreateCityRegionManager(cityRegionsCount);
        }

        public void Unload()
        {
            RegionChanges.Close();
        }

        #region Map Region Methods

        public ushort GetTileType(uint x, uint y)
        {
            Region region = GetRegion(x, y);
            return region.GetTileType(x, y);
        }

        public ushort RevertTileType(uint x, uint y, bool sendEvent)
        {
            ushort regionId = Region.GetRegionIndex(x, y);
            ushort tileType;

            lock (RegionChanges)
            {
                Region region = GetRegion(x, y);

                long idx = (Region.GetTileIndex(x, y) * Region.TILE_SIZE) + (Region.TILE_SIZE * RegionSize * regionId);
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
            ushort regionId = Region.GetRegionIndex(x, y);

            lock (RegionChanges)
            {
                Region region = GetRegion(x, y);

                long idx = (Region.GetTileIndex(x, y) * Region.TILE_SIZE) + (Region.TILE_SIZE * RegionSize * regionId);
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

        private void Remove(ISimpleGameObject obj, uint origX, uint origY)
        {
            obj.InWorld = false;
            DeregisterObjectEventListeners(obj);

            Region region = GetRegion(origX, origY);

            if (region == null)
            {
                return;
            }

            // Remove from region
            ushort regionId = Region.GetRegionIndex(obj);
            region.Remove(obj);

            // Remove this object from minimap if needed
            ICityRegionObject cityRegionObject = obj as ICityRegionObject;
            if (cityRegionObject != null)
            {
                CityRegions.Remove(cityRegionObject);
            }

            channel.Post("/WORLD/" + regionId, () =>
                {
                    var packet = new Packet(Command.ObjectRemove);
                    packet.AddUInt16(regionId);
                    packet.AddUInt32(obj.GroupId);
                    packet.AddUInt32(obj.ObjectId);
                    return packet;
                });
        }
    }
}