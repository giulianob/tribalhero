#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using Game.Comm;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Map
{
    public class World
    {
        #region Members

        private readonly LargeIdGenerator cityIdGen = new LargeIdGenerator(uint.MaxValue);
        private readonly LargeIdGenerator objectIdGenerator = new LargeIdGenerator(int.MaxValue);
        private CityRegion[] cityRegions;
        private Region[] regions;
        public RoadManager RoadManager { get; private set; }

        public uint WorldWidth { get; private set; }

        public uint WorldHeight { get; private set; }

        public object Lock { get; private set; }

        public Dictionary<uint, Player> Players { get; private set; }

        public ForestManager Forests { get; private set; }

        private Dictionary<uint, City> Cities { get; set; }

        private int RegionsCount { get; set; }
        private uint RegionSize { get; set; }
        private Stream RegionChanges { get; set; }
        private byte[] MapData { get; set; }

        public int CityCount
        {
            get
            {
                return Cities.Count;
            }
        }

        #endregion

        public World()
        {
            Cities = new Dictionary<uint, City>();
            RoadManager = new RoadManager();
            Lock = new object();
            Players = new Dictionary<uint, Player>();
            Forests = new ForestManager();
        }

        #region Object Getters

        public bool TryGetObjects(uint cityId, out City city)
        {
            return Cities.TryGetValue(cityId, out city);
        }

        public bool TryGetObjects(uint playerId, out Player player)
        {
            return Players.TryGetValue(playerId, out player);
        }

        public bool TryGetObjects(uint cityId, byte troopStubId, out City city, out TroopStub troopStub)
        {
            troopStub = null;

            return Cities.TryGetValue(cityId, out city) && city.Troops.TryGetStub(troopStubId, out troopStub);
        }

        public bool TryGetObjects(uint cityId, uint structureId, out City city, out Structure structure)
        {
            structure = null;

            return Cities.TryGetValue(cityId, out city) && city.TryGetStructure(structureId, out structure);
        }

        public bool TryGetObjects(uint cityId, uint troopObjectId, out City city, out TroopObject troopObject)
        {
            troopObject = null;

            return Cities.TryGetValue(cityId, out city) && city.TryGetTroop(troopObjectId, out troopObject);
        }

        #endregion

        #region Properties

        public List<SimpleGameObject> this[uint x, uint y]
        {
            get
            {
                Region region = GetRegion(x, y);
                return region == null ? new List<SimpleGameObject>() : region.GetObjects(x, y);
            }
        }

        #endregion

        #region Logical Methods

        public void Unload()
        {
            RegionChanges.Close();
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
                throw new Exception("Missing map");

            if (regionChangesStream == null)
                throw new Exception("Missing region changes");

            if (regionWidth == 0 || regionHeight == 0 || inWorldWidth%regionWidth != 0 || inWorldHeight%regionHeight != 0)
                throw new Exception("Invalid region size configured");

            if (cityRegionWidth == 0 || cityRegionHeight == 0 || inWorldWidth%cityRegionWidth != 0 || inWorldHeight%cityRegionHeight != 0)
                throw new Exception("Invalid city region size configured");

            WorldWidth = inWorldWidth;
            WorldHeight = inWorldHeight;

            // creating regions;                    
            //Global.Logger.InfoFormat("Region width[{0}] Region height[{1}] City region width[{2}] City region height[{3}]", regionWidth, regionHeight, cityRegionWidth, cityRegionHeight);            

            RegionSize = regionWidth*regionHeight;

            var column = (int)(inWorldWidth/regionWidth);
            var row = (int)(inWorldHeight/regionHeight);

            RegionsCount = column*row;
            MapData = new byte[RegionSize*Region.TILE_SIZE*RegionsCount];

            RegionChanges = regionChangesStream;

            regions = new Region[RegionsCount];
            for (int regionId = 0; regionId < RegionsCount; ++regionId)
            {
                var data = new byte[RegionSize*Region.TILE_SIZE]; // 1 tile is 2 bytes

                var mapDataOffset = (int)(RegionSize*Region.TILE_SIZE*regionId);

                if (mapStream.Read(MapData, mapDataOffset, (int)(RegionSize*Region.TILE_SIZE)) != RegionSize*Region.TILE_SIZE)
                    throw new Exception("Not enough map data");

                if (createRegionChanges)
                {
                    Buffer.BlockCopy(MapData, mapDataOffset, data, 0, data.Length);
                    regionChangesStream.Write(data, 0, data.Length);
                }
                else
                {
                    if (RegionChanges.Read(data, 0, (int)(RegionSize*Region.TILE_SIZE)) != RegionSize*Region.TILE_SIZE)
                        throw new Exception("Not enough region change map data");
                }

                regions[regionId] = new Region(data);
            }

            Global.Logger.Info(string.Format("map file length[{0}] position[{1}]", mapStream.Length, mapStream.Position));
            Global.Logger.Info(regions.Length + " created.");

            // creating city regions;
            column = (int)(inWorldWidth/cityRegionWidth);
            row = (int)(inWorldHeight/cityRegionHeight);
            int cityRegionsCount = column*row;

            cityRegions = new CityRegion[cityRegionsCount];
            for (int regionId = 0; regionId < cityRegionsCount; ++regionId)
                cityRegions[regionId] = new CityRegion();
        }

        public bool Add(City city)
        {
            lock (Lock)
            {
                city.BeginUpdate();
                city.Id = (uint)cityIdGen.GetNext();
                city.EndUpdate();
                Cities[city.Id] = city;

                //Initial save of these objects
                Global.DbManager.Save((Structure)city[1]);
                foreach (var stub in city.Troops)
                    Global.DbManager.Save(stub);

                CityRegion region = GetCityRegion(city.X, city.Y);
                return region != null && region.Add(city);
            }
        }

        public void DbLoaderAdd(uint id, City city)
        {
            city.Id = id;
            Cities[city.Id] = city;
            cityIdGen.Set((int)id);
        }

        public void AfterDbLoaded()
        {
            IEnumerator<City> iter = Cities.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                // Resave city to update times
                Global.DbManager.Save(iter.Current);

                //Set resource cap
                Procedure.SetResourceCap(iter.Current);

                //Set up the city region (for minimap)
                CityRegion region = GetCityRegion(iter.Current.X, iter.Current.Y);
                if (region != null)
                    region.Add(iter.Current);
            }

            // Launch forest creator
            Global.World.Forests.StartForestCreator();
        }

        public void Remove(City city)
        {
            lock (Lock)
            {
                Cities[city.Id] = null;
                cityIdGen.Release((int)city.Id);
                CityRegion region = GetCityRegion(city.X, city.Y);

                if (region == null)
                    return;

                region.Remove(city);
            }
        }

        public bool Add(SimpleGameObject obj)
        {
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
                return false;

            if (region.Add(obj))
            {
                // Keeps track of objects that exist in the map
                obj.InWorld = true;

                // If simple object, we must assign an id
                if (!(obj is GameObject))
                    obj.ObjectId = (uint)objectIdGenerator.GetNext();
                else if (obj is Structure && !(ObjectTypeFactory.IsStructureType("NoRoadRequired", (Structure)obj)))
                    RoadManager.CreateRoad(obj.X, obj.Y);

                // Add appropriate objects to the minimap
                if (obj is ICityRegionObject)
                {
                    CityRegion cityRegion = GetCityRegion(obj.X, obj.Y);
                    if (cityRegion != null)
                        cityRegion.Add((ICityRegionObject)obj);
                }

                // Send obj add event
                if (Global.FireEvents)
                {
                    ushort regionId = Region.GetRegionIndex(obj);

                    var packet = new Packet(Command.ObjectAdd);
                    packet.AddUInt16(regionId);
                    PacketHelper.AddToPacket(obj, packet, true);

                    Global.Channel.Post("/WORLD/" + regionId, packet);
                }

                return true;
            }

            return false;
        }

        public void DbLoaderAdd(SimpleGameObject obj)
        {
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
                return;

            if (obj.InWorld)
                region.Add(obj);

            // Set id in use
            if (!(obj is GameObject))
                objectIdGenerator.Set((int)obj.ObjectId);

            // Add to minimap if needed
            if (obj is ICityRegionObject)
            {
                CityRegion cityRegion = GetCityRegion(obj.X, obj.Y);
                if (cityRegion != null)
                    cityRegion.Add((ICityRegionObject)obj);
            }
        }

        public void Remove(SimpleGameObject obj)
        {
            Remove(obj, obj.X, obj.Y);
        }

        private void Remove(SimpleGameObject obj, uint origX, uint origY)
        {
            obj.InWorld = false;

            Region region = GetRegion(origX, origY);

            if (region == null)
                return;

            // Remove from region
            ushort regionId = Region.GetRegionIndex(obj);
            region.Remove(obj);

            // Remove this object from minimap if needed
            if (obj is ICityRegionObject)
            {
                CityRegion cityRegion = GetCityRegion(obj.X, obj.Y);
                if (cityRegion != null)
                    cityRegion.Remove((ICityRegionObject)obj);
            }

            // Free object id if this is SimpleGameObject
            if (!(obj is GameObject))
                objectIdGenerator.Release((int)obj.ObjectId);

            // Send remove update
            if (Global.FireEvents)
            {
                var packet = new Packet(Command.ObjectRemove);
                packet.AddUInt16(regionId);
                if (obj is GameObject)
                    packet.AddUInt32(((GameObject)obj).City.Id);
                else
                    packet.AddUInt32(0);
                packet.AddUInt32(obj.ObjectId);

                Global.Channel.Post("/WORLD/" + regionId, packet);
            }
        }

        public List<SimpleGameObject> GetObjects(uint x, uint y)
        {
            Region region = GetRegion(x, y);
            return region == null ? null : region.GetObjects(x, y);
        }

        #endregion

        #region Events

        public void ObjectUpdateEvent(SimpleGameObject sender, uint origX, uint origY)
        {
            // If object is a city region object, we need to update that
            if (sender is ICityRegionObject)
            {
                var senderAsCityRegionObject = (ICityRegionObject)sender;

                ushort oldCityRegionId = CityRegion.GetRegionIndex(origX, origY);
                ushort newCityRegionId = CityRegion.GetRegionIndex(sender.X, sender.Y);

                if (oldCityRegionId == newCityRegionId)
                    cityRegions[oldCityRegionId].Update(senderAsCityRegionObject, origX, origY);
                else
                {
                    cityRegions[oldCityRegionId].Remove(senderAsCityRegionObject);
                    cityRegions[newCityRegionId].Add(senderAsCityRegionObject);
                }
            }

            //if object has moved then we need to do some logic to see if it has changed regions
            ushort oldRegionId = Region.GetRegionIndex(origX, origY);
            ushort newRegionId = Region.GetRegionIndex(sender);

            //object has not changed regions so simply update
            if (oldRegionId == newRegionId)
            {
                regions[newRegionId].Update(sender, origX, origY);
                var packet = new Packet(Command.ObjectUpdate);
                packet.AddUInt16(newRegionId);
                PacketHelper.AddToPacket(sender, packet, true);
                Global.Channel.Post("/WORLD/" + newRegionId, packet);
            }
            // object has changed regions, need to remove it from the old one and add it to the new one
            else
            {
                regions[oldRegionId].Remove(sender, origX, origY);
                regions[newRegionId].Add(sender);
                var packet = new Packet(Command.ObjectMove);
                packet.AddUInt16(oldRegionId);
                packet.AddUInt16(newRegionId);
                PacketHelper.AddToPacket(sender, packet, true);
                Global.Channel.Post("/WORLD/" + oldRegionId, packet);

                packet = new Packet(Command.ObjectAdd);
                packet.AddUInt16(newRegionId);
                PacketHelper.AddToPacket(sender, packet, true);
                Global.Channel.Post("/WORLD/" + newRegionId, packet);
            }            
        }

        #endregion

        #region Helpers

        public bool IsValidXandY(uint x, uint y)
        {
            return x < Config.map_width && y < Config.map_height;
        }

        public Region GetRegion(uint x, uint y)
        {
            if (x == 0 && y == 0)
                return null;

            return GetRegion(Region.GetRegionIndex(x, y));
        }

        public Region GetRegion(ushort id)
        {
            if (id >= regions.Length)
                return null;
            return regions[id];
        }

        public CityRegion GetCityRegion(uint x, uint y)
        {
            if (x == 0 && y == 0)
                return null;

            return GetCityRegion(CityRegion.GetRegionIndex(x, y));
        }

        public CityRegion GetCityRegion(ushort id)
        {
            return cityRegions[id];
        }

        #endregion

        #region Channel Subscriptions

        internal void SubscribeRegion(Session session, ushort id)
        {
            try
            {
                Global.Channel.Subscribe(session, "/WORLD/" + id);
            }
            catch(DuplicateSubscriptionException)
            {
            }
        }

        internal void UnsubscribeRegion(Session session, ushort id)
        {
            Global.Channel.Unsubscribe(session, "/WORLD/" + id);
        }

        #endregion

        internal void LockRegion(uint x, uint y)
        {
            Monitor.Enter(GetRegion(x, y).Lock);
        }

        internal void UnlockRegion(uint x, uint y)
        {
            Monitor.Exit(GetRegion(x, y).Lock);
        }

        private bool GetObjectsForeach(uint ox, uint oy, uint x, uint y, object custom)
        {
            if (x < WorldWidth && y < WorldHeight)
                ((List<SimpleGameObject>)custom).AddRange(GetObjects(x, y));
            return true;
        }

        public List<SimpleGameObject> GetObjectsWithin(uint x, uint y, byte radius)
        {
            var list = new List<SimpleGameObject>();
            TileLocator.ForeachObject(x, y, radius, false, GetObjectsForeach, list);
            return list;
        }

        private bool GetTilesForeach(uint ox, uint oy, uint x, uint y, object custom)
        {
            if (x < WorldWidth && y < WorldHeight)
                ((List<ushort>)custom).Add(GetTileType(x, y));
            return true;
        }

        public List<ushort> GetTilesWithin(uint x, uint y, byte radius)
        {
            var list = new List<ushort>();
            TileLocator.ForeachObject(x, y, radius, false, GetTilesForeach, list);
            return list;
        }

        public bool FindPlayerId(string name, out uint playerId)
        {
            playerId = ushort.MaxValue;
            using (
                    DbDataReader reader = Global.DbManager.ReaderQuery(string.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Player.DB_TABLE),
                                                                       new[] {new DbColumn("name", name, DbType.String)}))
            {
                if (!reader.HasRows)
                    return false;
                reader.Read();
                playerId = (uint)reader[0];
                return true;
            }
        }

        public bool FindCityId(string name, out uint cityId)
        {
            cityId = ushort.MaxValue;
            using (
                    DbDataReader reader = Global.DbManager.ReaderQuery(string.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", City.DB_TABLE),
                                                                       new[] {new DbColumn("name", name, DbType.String)}))
            {
                if (!reader.HasRows)
                    return false;
                reader.Read();
                cityId = (uint)reader[0];
                return true;
            }
        }

        public bool CityNameTaken(string name)
        {
            using (
                    DbDataReader reader = Global.DbManager.ReaderQuery(string.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", City.DB_TABLE),
                                                                       new[] {new DbColumn("name", name, DbType.String)}))
            {
                return reader.HasRows;
            }
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

                long idx = (Region.GetTileIndex(x, y)*Region.TILE_SIZE) + (Region.TILE_SIZE*RegionSize*regionId);
                tileType = MapData[idx];
                RegionChanges.Seek(idx, SeekOrigin.Begin);
                RegionChanges.Write(BitConverter.GetBytes(tileType), 0, 2);
                RegionChanges.Flush();

                region.SetTileType(x, y, tileType);
            }

            if (sendEvent && Global.FireEvents)
            {
                var packet = new Packet(Command.RegionSetTile);
                packet.AddUInt16(1);
                packet.AddUInt32(x);
                packet.AddUInt32(y);
                packet.AddUInt16(tileType);

                Global.Channel.Post("/WORLD/" + regionId, packet);
            }

            return tileType;
        }

        public void SetTileType(uint x, uint y, ushort tileType, bool sendEvent)
        {
            ushort regionId = Region.GetRegionIndex(x, y);

            lock (RegionChanges)
            {
                Region region = GetRegion(x, y);

                long idx = (Region.GetTileIndex(x, y)*Region.TILE_SIZE) + (Region.TILE_SIZE*RegionSize*regionId);
                RegionChanges.Seek(idx, SeekOrigin.Begin);
                RegionChanges.Write(BitConverter.GetBytes(tileType), 0, 2);
                RegionChanges.Flush();

                region.SetTileType(x, y, tileType);
            }

            if (sendEvent && Global.FireEvents)
            {
                var packet = new Packet(Command.RegionSetTile);
                packet.AddUInt16(1);
                packet.AddUInt32(x);
                packet.AddUInt32(y);
                packet.AddUInt16(tileType);

                Global.Channel.Post("/WORLD/" + regionId, packet);
            }
        }

        #endregion
    }
}