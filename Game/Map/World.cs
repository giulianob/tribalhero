#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Game.Comm;
using Game.Data;
using Game.Data.Troop;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Map {
    public class World {
        #region Members

        private uint worldWidth;
        public uint WorldWidth {
            get { return worldWidth; }
        }

        private uint worldHeight;
        public uint WorldHeight {
            get { return worldHeight; }
        }

        private Region[] regions;
        private CityRegion[] cityRegions;
        private readonly Dictionary<uint, City> cities = new Dictionary<uint, City>();
        private readonly LargeIdGenerator cityIdGen = new LargeIdGenerator(uint.MaxValue);

        #endregion

        #region Object Getters

        public bool TryGetObjects(uint cityId, out City city) {
            return cities.TryGetValue(cityId, out city);
        }

        public bool TryGetObjects(uint cityId, byte troopStubId, out City city, out TroopStub troopStub) {
            troopStub = null;

            if (!cities.TryGetValue(cityId, out city))
                return false;

            if (!city.Troops.TryGetStub(troopStubId, out troopStub))
                return false;

            return true;
        }

        public bool TryGetObjects(uint cityId, uint structureId, out City city, out Structure structure) {
            structure = null;

            if (!cities.TryGetValue(cityId, out city))
                return false;

            if (!city.TryGetStructure(structureId, out structure))
                return false;

            return true;
        }

        public bool TryGetObjects(uint cityId, uint troopObjectId, out City city, out TroopObject troopObject) {
            troopObject = null;

            if (!cities.TryGetValue(cityId, out city))
                return false;

            if (!city.TryGetTroop(troopObjectId, out troopObject))
                return false;

            return true;
        }

        #endregion

        #region Properties

        public List<GameObject> this[uint x, uint y] {
            get { return GetRegion(x, y).getObjects(x, y); }
        }

        #endregion

        #region Constructors

        #endregion

        #region Logical Methods

        public bool Load(Stream stream, uint inWorldWidth, uint inWorldHeight, uint regionWidth, uint regionHeight,
                         uint cityRegionWidth, uint cityRegionHeight) {
            if (stream == null)
                return false;
            //        if (stream.Length != world_width * world_height) return false;
            if (inWorldWidth%regionWidth != 0 || inWorldHeight%regionHeight != 0)
                return false;

            worldWidth = inWorldWidth;
            worldHeight = inWorldHeight;

            // creating regions;
            uint regionSize = regionWidth*regionHeight;
            int column = (int) (inWorldWidth/regionWidth);
            int row = (int) (inWorldHeight/regionHeight);
            int regionsCount = column*row;

            regions = new Region[regionsCount];
            for (int regionId = 0; regionId < regionsCount; ++regionId) {
                Byte[] data = new Byte[regionSize*Region.TILE_SIZE]; // 1 tile is 2 bytes
                if (stream.Read(data, 0, (int) (regionSize*Region.TILE_SIZE)) != regionSize*Region.TILE_SIZE) {
                    Global.Logger.Error("Not enough map data");
                    return false;
                }
                regions[regionId] = new Region(data);
            }

            Global.Logger.Info(string.Format("map file length[{0}] position[{1}]", stream.Length, stream.Position));
            Global.Logger.Info(regions.Length + " created.");

            // creating city regions;
            column = (int) (inWorldWidth/cityRegionWidth);
            row = (int) (inWorldHeight/cityRegionHeight);
            regionsCount = column*row;

            cityRegions = new CityRegion[regionsCount];
            for (int regionId = 0; regionId < regionsCount; ++regionId)
                cityRegions[regionId] = new CityRegion();

            return true;
        }

        public bool Add(City city) {
            city.BeginUpdate();
            city.Id = (uint) cityIdGen.getNext();
            city.EndUpdate();
            cities[city.Id] = city;

            //Initial save of these objects
            Global.DbManager.Save(city.MainBuilding);
            foreach (TroopStub stub in city.Troops)
                Global.DbManager.Save(stub);

            CityRegion region = GetCityRegion(city.MainBuilding.X, city.MainBuilding.Y);
            if (region == null)
                return false;

            return region.Add(city);
        }

        public void DbLoaderAdd(uint id, City city) {
            city.Id = id;
            cities[city.Id] = city;
            cityIdGen.set((int) id);
        }

        public void AfterDbLoaded() {
            IEnumerator<City> iter = cities.Values.GetEnumerator();
            while (iter.MoveNext()) {
                //Set resource cap
                Formula.ResourceCap(iter.Current);

                //Set up the city region (for minimap)
                CityRegion region = GetCityRegion(iter.Current.MainBuilding.X, iter.Current.MainBuilding.Y);
                if (region == null)
                    continue;
                region.Add(iter.Current);
            }
        }

        public void Remove(City city) {
            cities[city.Id] = null;
            cityIdGen.release((int) city.Id);
            CityRegion region = GetCityRegion(city.MainBuilding.X, city.MainBuilding.Y);
            if (region == null)
                return;
            region.Remove(city);
        }

        public bool Add(GameObject obj) {
            Region region = GetRegion(obj.X, obj.Y);
            if (region == null)
                return false;

            if (region.add(obj)) {
                ushort regionId = Region.getRegionIndex(obj);
                Packet packet = new Packet(Command.OBJECT_ADD);
                packet.AddUInt16(regionId);
                PacketHelper.AddToPacket(obj, packet, true);

                if (Global.FireEvents)
                    Global.Channel.Post("/WORLD/" + regionId, packet);
                return true;
            }

            return false;
        }

        public void Remove(GameObject obj) {
            Remove(obj, obj.X, obj.Y);
        }

        private void Remove(GameObject obj, uint origX, uint origY) {
            Region region = GetRegion(origX, origY);

            if (region == null)
                return;

            ushort regionId = Region.getRegionIndex(obj);

            Packet packet = new Packet(Command.OBJECT_REMOVE);
            packet.AddUInt16(regionId);
            packet.AddUInt32(obj.City.Id);
            packet.AddUInt32(obj.ObjectId);

            region.remove(obj);
            Global.Channel.Post("/WORLD/" + regionId, packet);
        }

        public List<GameObject> GetObjects(uint x, uint y) {
            Region region = GetRegion(x, y);
            if (region == null)
                return null;
            return region.getObjects(x, y);
        }

        #endregion

        #region Events

        public void ObjectUpdateEvent(GameObject sender, uint origX, uint origY) {
            //Check if object has moved
            if (sender.X != origX || sender.Y != origY) {
                //if object has moved then we need to do some logic to see if it has changed regions
                ushort oldRegionId = Region.getRegionIndex(origX, origY);
                ushort newRegionId = Region.getRegionIndex(sender);

                //object has not changed regions so simply update
                if (oldRegionId == newRegionId) {
                    regions[newRegionId].update(sender, origX, origY);
                    Packet packet = new Packet(Command.OBJECT_UPDATE);
                    packet.AddUInt16(newRegionId);
                    PacketHelper.AddToPacket(sender, packet, true);
                    Global.Channel.Post("/WORLD/" + newRegionId, packet);
                } else {
                    regions[oldRegionId].remove(sender, origX, origY);
                    regions[newRegionId].add(sender);
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
                ushort regionId = Region.getRegionIndex(sender);
                regions[regionId].update(sender, sender.X, sender.Y);
                Packet packet = new Packet(Command.OBJECT_UPDATE);
                packet.AddUInt16(regionId);
                PacketHelper.AddToPacket(sender, packet, true);
                Global.Channel.Post("/WORLD/" + regionId, packet);
            }

            //Handles updating city region information
            Structure structObject = sender as Structure;
            if (structObject != null && structObject.City.MainBuilding == structObject) {
                //If object is the main building then we need to update the city region
                CityRegion region = GetCityRegion(origX, origY);
                if (region != null)                    
                    region.Remove(structObject.City);

                region = GetCityRegion(structObject.X, structObject.Y);
                if (region != null)
                    region.Add(structObject.City);
            }
        }

        #endregion

        #region Helpers

        public Region GetRegion(uint x, uint y) {
            return GetRegion(Region.getRegionIndex(x, y));
        }

        public Region GetRegion(ushort id) {
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
            Global.Channel.Subscribe(session, "/WORLD/" + id);
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
                ((List<GameObject>) custom).AddRange(GetObjects(x, y));
            return true;
        }

        public List<GameObject> GetObjectsWithin(uint x, uint y, byte radius) {
            List<GameObject> list = new List<GameObject>();
            RadiusLocator.foreach_object(x, y, radius, false, GetObjectsForeach, list);
            return list;
        }

        public ushort GetTileType(uint x, uint y) {
            Region region = GetRegion(x, y);
            return region.getTileType(x, y);
        }
    }
}