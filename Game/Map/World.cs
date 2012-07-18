#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using Game.Comm;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Procedures;
using Game.Module.Remover;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;
using System.Linq;
#endregion

namespace Game.Map
{
    public class World : IGameObjectLocator, IWorld
    {
        public static World Current { get; set; }

        #region Members

        private readonly LargeIdGenerator cityIdGen = new LargeIdGenerator(UInt32.MaxValue);        
        private CityRegion[] cityRegions;
        private Region[] regions;
        public RoadManager RoadManager { get; private set; }

        private readonly LargeIdGenerator tribeIdGen = new LargeIdGenerator(UInt32.MaxValue);

        public uint WorldWidth { get; private set; }

        public uint WorldHeight { get; private set; }

        public object Lock { get; private set; }

        public Dictionary<uint, IPlayer> Players { get; private set; }

        public ForestManager Forests { get; private set; }

        private Dictionary<uint, ICity> Cities { get; set; }

        private Dictionary<uint, ITribe> Tribes { get; set; }

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

        public int TribeCount
        {
            get
            {
                return Tribes.Count;
            }
        }

        public int GetActivePlayerCount()
        {
            return new ActivePlayerSelector(Config.idle_days).GetPlayerIds().Count();
        }

        #endregion

        public World()
        {
            Cities = new Dictionary<uint, ICity>();
            RoadManager = new RoadManager();
            Lock = new object();
            Players = new Dictionary<uint, IPlayer>();
            Forests = new ForestManager();
            Tribes = new Dictionary<uint, ITribe>();
        }

        #region Object Getters

        public bool TryGetObjects(uint cityId, out ICity city)
        {
            return Cities.TryGetValue(cityId, out city);
        }

        public bool TryGetObjects(uint playerId, out IPlayer player)
        {
            return Players.TryGetValue(playerId, out player);
        }

        public bool TryGetObjects(uint tribeId, out ITribe tribe)
        {
            return Tribes.TryGetValue(tribeId, out tribe);
        }

        public bool TryGetObjects(uint cityId, byte troopStubId, out ICity city, out ITroopStub troopStub)
        {
            troopStub = null;

            return Cities.TryGetValue(cityId, out city) && city.Troops.TryGetStub(troopStubId, out troopStub);
        }

        public bool TryGetObjects(uint cityId, uint structureId, out ICity city, out IStructure structure)
        {
            structure = null;

            return Cities.TryGetValue(cityId, out city) && city.TryGetStructure(structureId, out structure);
        }

        public bool TryGetObjects(uint cityId, uint troopObjectId, out ICity city, out ITroopObject troopObject)
        {
            troopObject = null;

            return Cities.TryGetValue(cityId, out city) && city.TryGetTroop(troopObjectId, out troopObject);
        }

        public bool TryGetObjects(uint cityId, out ICity city, out ITribe tribe)
        {
            tribe = null;
            if (Cities.TryGetValue(cityId, out city))
            {
                if (city.Owner.IsInTribe)
                {
                    tribe = city.Owner.Tribesman.Tribe;
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Properties

        public List<ISimpleGameObject> this[uint x, uint y]
        {
            get
            {
                Region region = GetRegion(x, y);
                return region == null ? new List<ISimpleGameObject>() : region.GetObjects(x, y);
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

            Global.Logger.Info(String.Format("map file length[{0}] position[{1}]", mapStream.Length, mapStream.Position));
            Global.Logger.Info(regions.Length + " created.");

            // creating city regions;
            column = (int)(inWorldWidth/cityRegionWidth);
            row = (int)(inWorldHeight/cityRegionHeight);
            int cityRegionsCount = column*row;

            cityRegions = new CityRegion[cityRegionsCount];
            for (int regionId = 0; regionId < cityRegionsCount; ++regionId)
                cityRegions[regionId] = new CityRegion();
        }

        public void Add(ITribe tribe)
        {
            lock (Lock)
            {
                tribe.Id = (uint)tribeIdGen.GetNext();
                Tribes.Add(tribe.Id, tribe);
                DbPersistance.Current.Save(tribe);
            }
        }

        public void DbLoaderAdd(ITribe tribe)
        {
            lock (Lock)
            {
                tribeIdGen.Set(tribe.Id);
                Tribes.Add(tribe.Id, tribe);
            }
        }

        public bool Add(ICity city)
        {
            lock (Lock)
            {
                city.BeginUpdate();
                city.Id = (uint)cityIdGen.GetNext();
                city.EndUpdate();
                Cities[city.Id] = city;

                //Initial save of these objects
                DbPersistance.Current.Save((IStructure)city[1]);
                foreach (var stub in city.Troops)
                    DbPersistance.Current.Save(stub);

                CityRegion region = GetCityRegion(city.X, city.Y);
                return region != null && region.Add(city);
            }
        }

        public void DbLoaderAdd(uint id, ICity city)
        {
            city.Id = id;
            cityIdGen.Set((int)id);

            if (city.Deleted != City.DeletedState.Deleted)
                Cities.Add(city.Id, city);            
        }

        public void AfterDbLoaded()
        {
            IEnumerator<ICity> iter = Cities.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                // Resave city to update times
                DbPersistance.Current.Save(iter.Current);

                //Set resource cap
                Procedure.Current.SetResourceCap(iter.Current);

                //Set up the city region (for minimap)
                CityRegion region = GetCityRegion(iter.Current.X, iter.Current.Y);
                if (region != null)
                    region.Add(iter.Current);
            }

            // Launch forest creator
            Current.Forests.StartForestCreator();
        }

        public void Remove(ICity city)
        {
            lock (Lock)
            {
                city.BeginUpdate();
                DbPersistance.Current.DeleteDependencies(city);
                city.Deleted = City.DeletedState.Deleted;
                city.EndUpdate();

                Cities.Remove(city.Id);
            }
        }

        public void Remove(ITribe tribe)
        {
            lock (Lock)
            {
                Tribes.Remove(tribe.Id);
                DbPersistance.Current.Delete(tribe);
            }
        }

        public bool Add(ISimpleGameObject obj)
        {
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
                return false;

            if (region.Add(obj))
            {
                // Keeps track of objects that exist in the map
                obj.InWorld = true;

                // If simple object, we must assign an id
                if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("NoRoadRequired", obj.Type))
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

        public void DbLoaderAdd(ISimpleGameObject obj)
        {
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
                return;

            if (obj.InWorld)
                region.Add(obj);

            // Add to minimap if needed
            if (obj is ICityRegionObject)
            {
                CityRegion cityRegion = GetCityRegion(obj.X, obj.Y);
                if (cityRegion != null)
                    cityRegion.Add((ICityRegionObject)obj);
            }
        }

        public void Remove(ISimpleGameObject obj)
        {
            Remove(obj, obj.X, obj.Y);
        }

        private void Remove(ISimpleGameObject obj, uint origX, uint origY)
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

            // Send remove update
            if (Global.FireEvents)
            {
                var packet = new Packet(Command.ObjectRemove);
                packet.AddUInt16(regionId);
                packet.AddUInt32(obj.GroupId);
                packet.AddUInt32(obj.ObjectId);

                Global.Channel.Post("/WORLD/" + regionId, packet);
            }
        }

        public List<ISimpleGameObject> GetObjects(uint x, uint y)
        {
            Region region = GetRegion(x, y);
            return region == null ? new List<ISimpleGameObject>() : region.GetObjects(x, y);
        }

        #endregion

        #region Events

        public void ObjectUpdateEvent(ISimpleGameObject sender, uint origX, uint origY)
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
            GetRegion(x, y).Lock.EnterWriteLock();
        }

        internal void UnlockRegion(uint x, uint y)
        {
            GetRegion(x, y).Lock.ExitWriteLock();
        }

        private bool GetObjectsForeach(uint ox, uint oy, uint x, uint y, object custom)
        {
            if (x < WorldWidth && y < WorldHeight)
                ((List<ISimpleGameObject>)custom).AddRange(GetObjects(x, y));
            return true;
        }

        public List<ISimpleGameObject> GetObjectsWithin(uint x, uint y, byte radius)
        {
            var list = new List<ISimpleGameObject>();
            TileLocator.Current.ForeachObject(x, y, radius, false, GetObjectsForeach, list);
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
            TileLocator.Current.ForeachObject(x, y, radius, false, GetTilesForeach, list);
            return list;
        }

        public bool FindPlayerId(string name, out uint playerId)
        {
            playerId = UInt16.MaxValue;
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Player.DB_TABLE),
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
            cityId = UInt16.MaxValue;
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", City.DB_TABLE),
                                                                       new[] {new DbColumn("name", name, DbType.String)}))
            {
                if (!reader.HasRows)
                    return false;
                reader.Read();
                cityId = (uint)reader[0];
                return true;
            }
        }

        public bool FindTribeId(string name, out uint tribeId) {
            tribeId = UInt16.MaxValue;
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Tribe.DB_TABLE),
                                                                       new[] { new DbColumn("name", name, DbType.String) })) {
                if (!reader.HasRows)
                    return false;
                reader.Read();
                tribeId = (uint)reader[0];
                return true;
            }
        }

        public bool CityNameTaken(string name)
        {
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", City.DB_TABLE),
                                                                       new[] {new DbColumn("name", name, DbType.String)}))
            {
                return reader.HasRows;
            }
        }

        public bool TribeNameTaken(string name)
        {
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Tribe.DB_TABLE),
                                                                       new[] { new DbColumn("name", name, DbType.String) }))
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

                // Check if it's actually changed
                if (region.GetTileType(x, y) == tileType)
                    return tileType;

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