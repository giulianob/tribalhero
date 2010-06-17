#region

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using Game.Comm;
using Game.Data;
using Game.Data.Troop;
using Game.Logic;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Map {
    public class World {
        #region Members

        public RoadManager RoadManager { get; private set; }

        public uint WorldWidth { get; private set; }

        public uint WorldHeight { get; private set; }

        public object Lock { get; private set; }

        public LargeIdGenerator ObjectIdGenerator { get; private set; }

        public int CityCount {
            get { return cities.Count; }
        }
        
        private Region[] regions;
        private CityRegion[] cityRegions;
        private readonly Dictionary<uint, City> cities = new Dictionary<uint, City>();
        private readonly LargeIdGenerator cityIdGen = new LargeIdGenerator(uint.MaxValue);

        private int RegionsCount { get; set; }
        private uint RegionSize { get; set; }
        private Stream RegionChanges { get; set; }
        private byte[] MapData { get; set; }

        #endregion

        public World() {
            RoadManager = new RoadManager();
            Lock = new object();
            ObjectIdGenerator = new LargeIdGenerator(int.MaxValue);
        }

        #region Object Getters

        public bool TryGetObjects(uint cityId, out City city) {
            return cities.TryGetValue(cityId, out city);
        }

        public bool TryGetObjects(uint cityId, byte troopStubId, out City city, out TroopStub troopStub) {
            troopStub = null;

            return cities.TryGetValue(cityId, out city) && city.Troops.TryGetStub(troopStubId, out troopStub);
        }

        public bool TryGetObjects(uint cityId, uint structureId, out City city, out Structure structure) {
            structure = null;

            return cities.TryGetValue(cityId, out city) && city.TryGetStructure(structureId, out structure);
        }

        public bool TryGetObjects(uint cityId, uint troopObjectId, out City city, out TroopObject troopObject) {
            troopObject = null;

            return cities.TryGetValue(cityId, out city) && city.TryGetTroop(troopObjectId, out troopObject);
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

        public void Unload() {            
            RegionChanges.Close();
        }

        public void Load(Stream mapStream, Stream regionChangesStream, bool createRegionChanges, uint inWorldWidth, uint inWorldHeight, uint regionWidth, uint regionHeight,
                         uint cityRegionWidth, uint cityRegionHeight) {
            if (mapStream == null)
                throw new Exception("Missing map");

            if (regionChangesStream == null)
                throw new Exception("Missing region changes");

            if (inWorldWidth % regionWidth != 0 || inWorldHeight % regionHeight != 0)
                throw new Exception("Invalid region size configured");

            WorldWidth = inWorldWidth;
            WorldHeight = inWorldHeight;

            // creating regions;            
            RegionSize = regionWidth*regionHeight;
            int column = (int) (inWorldWidth/regionWidth);
            int row = (int) (inWorldHeight/regionHeight);
            RegionsCount = column*row;
            MapData = new byte[RegionSize * Region.TILE_SIZE * RegionsCount];

            RegionChanges = regionChangesStream;

            regions = new Region[RegionsCount];
            for (int regionId = 0; regionId < RegionsCount; ++regionId) {
                
                byte[] data = new byte[RegionSize*Region.TILE_SIZE]; // 1 tile is 2 bytes
                
                int mapDataOffset = (int)(RegionSize * Region.TILE_SIZE * regionId);

                if (mapStream.Read(MapData, mapDataOffset, (int)(RegionSize * Region.TILE_SIZE)) != RegionSize * Region.TILE_SIZE)
                    throw new Exception("Not enough map data");

                if (createRegionChanges) {
                    Buffer.BlockCopy(MapData, mapDataOffset, data, 0, data.Length);
                    regionChangesStream.Write(data, 0, data.Length);
                } else {
                    if (RegionChanges.Read(data, 0, (int)(RegionSize * Region.TILE_SIZE)) != RegionSize * Region.TILE_SIZE)
                        throw new Exception("Not enough region change map data");
                }

                regions[regionId] = new Region(data);
            }

            Global.Logger.Info(string.Format("map file length[{0}] position[{1}]", mapStream.Length, mapStream.Position));
            Global.Logger.Info(regions.Length + " created.");

            // creating city regions;
            column = (int) (inWorldWidth/cityRegionWidth);
            row = (int) (inWorldHeight/cityRegionHeight);
            int cityRegionsCount = column*row;

            cityRegions = new CityRegion[cityRegionsCount];
            for (int regionId = 0; regionId < cityRegionsCount; ++regionId)
                cityRegions[regionId] = new CityRegion();
        }

        public bool Add(City city) {
            lock (Lock) {
                city.BeginUpdate();
                city.Id = (uint) cityIdGen.GetNext();
                city.EndUpdate();
                cities[city.Id] = city;

                //Initial save of these objects
                Global.DbManager.Save(city.MainBuilding);
                foreach (TroopStub stub in city.Troops)
                    Global.DbManager.Save(stub);

                CityRegion region = GetCityRegion(city.MainBuilding.X, city.MainBuilding.Y);
                return region != null && region.Add(city);
            }
        }

        public void DbLoaderAdd(uint id, City city) {
            city.Id = id;
            cities[city.Id] = city;
            cityIdGen.Set((int) id);
        }

        public void AfterDbLoaded() {
            IEnumerator<City> iter = cities.Values.GetEnumerator();
            while (iter.MoveNext()) {

                // Resave city to update times
                Global.DbManager.Save(iter.Current);

                //Set resource cap
                Formula.ResourceCap(iter.Current);

                //Set up the city region (for minimap)
                CityRegion region = GetCityRegion(iter.Current.MainBuilding.X, iter.Current.MainBuilding.Y);
                if (region == null)
                    continue;

                region.Add(iter.Current);
            }

            // Launch forest creator
            Global.Forests.StartForestCreator();
        }

        public void Remove(City city) {
            lock (Lock) {
                cities[city.Id] = null;
                cityIdGen.Release((int) city.Id);
                CityRegion region = GetCityRegion(city.MainBuilding.X, city.MainBuilding.Y);
                
                if (region == null)
                    return;

                region.Remove(city);
            }
        }

        public bool Add(SimpleGameObject obj) {
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
                return false;

            if (region.Add(obj)) {

                // Keeps track of objects that exist in the map
                obj.InWorld = true;

                // If simple object, we must assign an id
                if (!(obj is GameObject)) {
                    obj.ObjectId = (uint)ObjectIdGenerator.GetNext();
                } else if (obj is Structure && !(ObjectTypeFactory.IsStructureType("NoRoadRequired", (Structure)obj))) {
                    RoadManager.CreateRoad(obj.X, obj.Y);
                }

                // Send obj add event
                if (Global.FireEvents) {
                    ushort regionId = Region.GetRegionIndex(obj);

                    Packet packet = new Packet(Command.OBJECT_ADD);                    
                    packet.AddUInt16(regionId);
                    PacketHelper.AddToPacket(obj, packet, true);

                    Global.Channel.Post("/WORLD/" + regionId, packet);
                }                

                return true;
            }

            return false;
        }

        public void DbLoaderAdd(SimpleGameObject obj) {
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
                return;

            if (obj.InWorld)
                region.Add(obj);

            // Set id in use
            if (!(obj is GameObject)) {
                ObjectIdGenerator.Set((int)obj.ObjectId);
            }
        }

        public void Remove(SimpleGameObject obj) {
            Remove(obj, obj.X, obj.Y);            
        }

        private void Remove(SimpleGameObject obj, uint origX, uint origY) {
            obj.InWorld = false;

            Region region = GetRegion(origX, origY);

            if (region == null)
                return;

            // Remove from region
            ushort regionId = Region.GetRegionIndex(obj);
            region.Remove(obj);

            // Free object id if this is SimpleGameObject
            if (!(obj is GameObject)) {
                ObjectIdGenerator.Release((int) obj.ObjectId);
            }

            // Send remove update
            if (Global.FireEvents) {
                Packet packet = new Packet(Command.OBJECT_REMOVE);
                packet.AddUInt16(regionId);
                if (obj is GameObject)
                    packet.AddUInt32(((GameObject) obj).City.Id);
                else
                    packet.AddUInt32(0);
                packet.AddUInt32(obj.ObjectId);

                Global.Channel.Post("/WORLD/" + regionId, packet);
            }
        }

        public List<SimpleGameObject> GetObjects(uint x, uint y) {
            Region region = GetRegion(x, y);
            return region == null ? null : region.GetObjects(x, y);
        }

        #endregion

        #region Events

        public void ObjectUpdateEvent(SimpleGameObject sender, uint origX, uint origY) {
            //Check if object has moved
            if (sender.X != origX || sender.Y != origY) {
                //if object has moved then we need to do some logic to see if it has changed regions
                ushort oldRegionId = Region.GetRegionIndex(origX, origY);
                ushort newRegionId = Region.GetRegionIndex(sender);

                //object has not changed regions so simply update
                if (oldRegionId == newRegionId) {
                    regions[newRegionId].Update(sender, origX, origY);
                    Packet packet = new Packet(Command.OBJECT_UPDATE);
                    packet.AddUInt16(newRegionId);
                    PacketHelper.AddToPacket(sender, packet, true);
                    Global.Channel.Post("/WORLD/" + newRegionId, packet);
                } else {
                    regions[oldRegionId].Remove(sender, origX, origY);
                    regions[newRegionId].Add(sender);
                    Packet packet = new Packet(Command.OBJECT_MOVE);
                    packet.AddUInt16(oldRegionId);
                    packet.AddUInt16(newRegionId);
                    PacketHelper.AddToPacket(sender, packet, true);
                    Global.Channel.Post("/WORLD/" + oldRegionId, packet);

                    packet = new Packet(Command.OBJECT_ADD);
                    packet.AddUInt16(newRegionId);
                    PacketHelper.AddToPacket(sender, packet, true);
                    Global.Channel.Post("/WORLD/" + newRegionId, packet);
                }
            } else {
                ushort regionId = Region.GetRegionIndex(sender);
                regions[regionId].Update(sender, sender.X, sender.Y);
                Packet packet = new Packet(Command.OBJECT_UPDATE);
                packet.AddUInt16(regionId);
                PacketHelper.AddToPacket(sender, packet, true);
                Global.Channel.Post("/WORLD/" + regionId, packet);
            }

            //Handles updating city region information
            Structure structObject = sender as Structure;
            if (structObject == null || structObject.City.MainBuilding != structObject)
                return;

            //If object is the main building then we need to update the city region
            CityRegion region = GetCityRegion(origX, origY);
            if (region != null)                    
                region.Remove(structObject.City);

            region = GetCityRegion(structObject.X, structObject.Y);
            if (region != null)
                region.Add(structObject.City);
        }

        #endregion

        #region Helpers

        public bool IsValidXandY(uint x, uint y) {
            return x < Config.map_width && y < Config.map_height;
        }

        public Region GetRegion(uint x, uint y) {
            return GetRegion(Region.GetRegionIndex(x, y));
        }

        public Region GetRegion(ushort id) {
            if (id >= regions.Length) return null;
            return regions[id];
        }

        public CityRegion GetCityRegion(uint x, uint y) {
            return GetCityRegion(CityRegion.GetRegionIndex(x, y));
        }

        public CityRegion GetCityRegion(ushort id) {
            return cityRegions[id];
        }

        #endregion

        #region Channel Subscriptions

        internal void SubscribeRegion(Session session, ushort id) {
            try {
                Global.Channel.Subscribe(session, "/WORLD/" + id);
            }
            catch (DuplicateSubscriptionException) { }
        }

        internal void UnsubscribeRegion(Session session, ushort id) {
            Global.Channel.Unsubscribe(session, "/WORLD/" + id);
        }

        #endregion

        internal void LockRegion(uint x, uint y) {
            Monitor.Enter(GetRegion(x, y).Lock);
        }

        internal void UnlockRegion(uint x, uint y) {
            Monitor.Exit(GetRegion(x, y).Lock);
        }

        private bool GetObjectsForeach(uint ox, uint oy, uint x, uint y, object custom) {
            if (x < WorldWidth && y < WorldHeight)
                ((List<SimpleGameObject>) custom).AddRange(GetObjects(x, y));
            return true;
        }

        public List<SimpleGameObject> GetObjectsWithin(uint x, uint y, byte radius) {
            List<SimpleGameObject> list = new List<SimpleGameObject>();
            TileLocator.foreach_object(x, y, radius, false, GetObjectsForeach, list);
            return list;
        }

        private bool GetTilesForeach(uint ox, uint oy, uint x, uint y, object custom) {
            if (x < WorldWidth && y < WorldHeight)
                ((List<ushort>) custom).Add(GetTileType(x, y));
            return true;
        }

        public List<ushort> GetTilesWithin(uint x, uint y, byte radius) {
            List<ushort> list = new List<ushort>();
            TileLocator.foreach_object(x, y, radius, false, GetTilesForeach, list);
            return list;
        }

        #region Map Region Methods
        public ushort GetTileType(uint x, uint y) {
            Region region = GetRegion(x, y);            
            return region.GetTileType(x, y);
        }

        public ushort RevertTileType(uint x, uint y, bool sendEvent) {
            ushort regionId = Region.GetRegionIndex(x, y);
            ushort tileType;

            lock (RegionChanges) {
                Region region = GetRegion(x, y);

                long idx = (Region.GetTileIndex(x, y) * Region.TILE_SIZE) + (Region.TILE_SIZE * RegionSize * regionId);
                tileType = MapData[idx];
                RegionChanges.Seek(idx, SeekOrigin.Begin);
                RegionChanges.Write(BitConverter.GetBytes(tileType), 0, 2);
                RegionChanges.Flush();

                region.SetTileType(x, y, tileType);
            }

            if (sendEvent && Global.FireEvents) {
                Packet packet = new Packet(Command.REGION_SET_TILE);
                packet.AddUInt16(1);
                packet.AddUInt32(x);
                packet.AddUInt32(y);
                packet.AddUInt16(tileType);

                Global.Channel.Post("/WORLD/" + regionId, packet);
            }

            return tileType;
        }

        public void SetTileType(uint x, uint y, ushort tileType, bool sendEvent) {
            ushort regionId = Region.GetRegionIndex(x, y);

            lock (RegionChanges) {
                Region region = GetRegion(x, y);

                long idx = (Region.GetTileIndex(x, y) * Region.TILE_SIZE) + (Region.TILE_SIZE * RegionSize * regionId);
                RegionChanges.Seek(idx, SeekOrigin.Begin);
                RegionChanges.Write(BitConverter.GetBytes(tileType), 0, 2);
                RegionChanges.Flush();

                region.SetTileType(x, y, tileType);
            }

            if (sendEvent && Global.FireEvents) {
                Packet packet = new Packet(Command.REGION_SET_TILE);
                packet.AddUInt16(1);
                packet.AddUInt32(x);
                packet.AddUInt32(y);
                packet.AddUInt16(tileType);

                Global.Channel.Post("/WORLD/" + regionId, packet);
            }
        }
        #endregion

        public bool CityNameTaken(string name) {
            DbDataReader reader = Global.DbManager.ReaderQuery(string.Format("SELECT `id` FROM `{0}` WHERE name = '{1}' LIMIT 1", City.DB_TABLE, name));
            bool exists = reader.HasRows;
            reader.Close();
            return exists;
        }
    }
}