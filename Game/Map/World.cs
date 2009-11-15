using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Game.Data;
using Game.Comm;
using Game.Util;
using Game.Map;
using Game.Setup;
using System.Threading;
using Game.Database;
using Game.Logic;
namespace Game {

    public class World {

        #region Members

        uint worldWidth;
        public uint WorldWidth {
            get { return worldWidth; }
        }

        uint worldHeight;
        public uint WorldHeight {
            get { return worldHeight; }
        }

        Region[] regions;
        CityRegion[] cityRegions;
        Dictionary<uint, City> cities = new Dictionary<uint, City>();
        LargeIdGenerator cityIdGen = new LargeIdGenerator(uint.MaxValue);

        Channel channel = new Channel();
        #endregion

        #region Object Getters
        public bool TryGetObjects(uint cityId, out City city) {
            return cities.TryGetValue(cityId, out city);
        }

        public bool TryGetObjects(uint cityId, byte troopStubId, out City city, out TroopStub troopStub) {
            troopStub = null;

            if (!cities.TryGetValue(cityId, out city)) return false;

            if (!city.Troops.TryGetStub(troopStubId, out troopStub)) return false;

            return true;
        }

        public bool TryGetObjects(uint cityId, uint structureId, out City city, out Structure structure) {
            structure = null;

            if (!cities.TryGetValue(cityId, out city)) return false;

            if (!city.tryGetStructure(structureId, out structure)) return false;

            return true;
        }

        public bool TryGetObjects(uint cityId, uint troopObjectId, out City city, out TroopObject troopObject) {
            troopObject = null;

            if (!cities.TryGetValue(cityId, out city)) return false;

            if (!city.tryGetTroop(troopObjectId, out troopObject)) return false;

            return true;
        }
        #endregion

        #region Properties
        public List<GameObject> this[uint x, uint y] {
            get {
                return getRegion(x, y).getObjects(x, y);
            }
        }

        #endregion

        #region Constructors
        public World() {
        }
        #endregion

        #region Logical Methods
        public bool load(Stream stream, uint worldWidth, uint worldHeight, uint regionWidth, uint regionHeight, uint cityRegionWidth, uint cityRegionHeight) {
            if (stream == null) return false;
            //        if (stream.Length != world_width * world_height) return false;
            if (worldWidth % regionWidth != 0 || worldHeight % regionHeight != 0) return false;


            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
            uint worldSize = worldWidth * worldHeight;

            // creating regions;
            uint regionSize = regionWidth * regionHeight;
            int column = (int)(worldWidth / regionWidth);
            int row = (int)(worldHeight / regionHeight);
            int regionsCount = column * row;

            regions = new Region[regionsCount];
            for (int region_id = 0; region_id < regionsCount; ++region_id) {
                Byte[] data = new Byte[regionSize * Region.TILE_SIZE]; // 1 tile is 2 bytes
                if (stream.Read(data, 0, (int)(regionSize * Region.TILE_SIZE)) != regionSize * Region.TILE_SIZE) {
                    Global.Logger.Error("Not enough map data");
                    return false;
                }
                regions[region_id] = new Region(data);
            }

            Global.Logger.Info(string.Format("map file length[{0}] position[{1}]", stream.Length, stream.Position));
            Global.Logger.Info(regions.Length + " created.");

            // creating city regions;
            regionSize = cityRegionWidth * cityRegionHeight;
            column = (int)(worldWidth / cityRegionWidth);
            row = (int)(worldHeight / cityRegionHeight);
            regionsCount = column * row;

            cityRegions = new CityRegion[regionsCount];
            for (int region_id = 0; region_id < regionsCount; ++region_id)
                cityRegions[region_id] = new CityRegion();

            return true;
        }

        public bool add(City city) {
            city.CityId = (uint)cityIdGen.getNext();
            cities[city.CityId] = city;
            Global.dbManager.Save(city, city.MainBuilding);
            foreach (TroopStub stub in city.Troops)
                Global.dbManager.Save(stub);

            CityRegion region = getCityRegion(city.MainBuilding.X, city.MainBuilding.Y);
            if (region == null) return false;
            return region.add(city);
        }

        public void dbLoaderAdd(uint id, City city) {
            city.CityId = id;
            cities[city.CityId] = city;
            cityIdGen.set((int)id);
        }

        public void afterDbLoaded() {
            IEnumerator<City> iter = cities.Values.GetEnumerator();
            while (iter.MoveNext()) {
                //Set resource cap
                Formula.ResourceCap(iter.Current);

                //Set up the city region (for minimap)
                CityRegion region = getCityRegion(iter.Current.MainBuilding.X, iter.Current.MainBuilding.Y);
                if (region == null) continue;
                region.add(iter.Current);
            }
        }

        public void remove(City city) {
            cities[city.CityId] = null;
            cityIdGen.release((int)city.CityId);
            CityRegion region = getCityRegion(city.MainBuilding.X, city.MainBuilding.Y);
            if (region == null) return;
            region.add(city);
        }

        public bool add(GameObject obj) {
            Region region = getRegion(obj.X, obj.Y);
            if (region == null) return false;

            if (region.add(obj)) {
                ushort region_id = Region.getRegionIndex(obj);
                Packet packet = new Packet(Command.OBJECT_ADD);
                packet.addUInt16(region_id);
                PacketHelper.AddToPacket(obj, packet, true);

                if (Global.FireEvents)
                    channel.post(region_id, packet);
                return true;
            }
            else {
                return false;
            }
        }

        public void remove(GameObject obj) {
            remove(obj, obj.X, obj.Y);
        }

        private void remove(GameObject obj, uint origX, uint origY) {
            Region region = getRegion(origX, origY);

            if (region == null)
                return;

            ushort region_id = Region.getRegionIndex(obj);

            Packet packet = new Packet(Command.OBJECT_REMOVE);
            packet.addUInt16(region_id);
            packet.addUInt32(obj.City.CityId);
            packet.addUInt32(obj.ObjectID);

            region.remove(obj);
            channel.post(region_id, packet);
        }

        public List<GameObject> getObjects(uint x, uint y) {
            Region region = getRegion(x, y);
            if (region == null) return null;
            return region.getObjects(x, y);
        }
        #endregion

        #region Events
        public void obj_UpdateEvent(GameObject sender, uint origX, uint origY) {
            //Check if object has moved
            if (sender.X != origX || sender.Y != origY) {
                //if object has moved then we need to do some logic to see if it has changed regions
                ushort oldRegionId = Region.getRegionIndex(origX, origY);
                ushort newRegionId = Region.getRegionIndex(sender);

                //object has not changed regions so simply update
                if (oldRegionId == newRegionId) {
                    regions[newRegionId].update(sender, origX, origY);
                    Packet packet = new Packet(Command.OBJECT_UPDATE);
                    packet.addUInt16(newRegionId);
                    PacketHelper.AddToPacket(sender, packet, true);
                    channel.post(newRegionId, packet);
                }
                else {
                    regions[oldRegionId].remove(sender, origX, origY);
                    regions[newRegionId].add(sender);
                    Packet packet = new Packet(Command.OBJECT_MOVE);
                    packet.addUInt16(oldRegionId);
                    packet.addUInt16(newRegionId);
                    PacketHelper.AddToPacket(sender, packet, true);
                    channel.post(oldRegionId, packet);

                    packet = new Packet(Command.OBJECT_ADD);
                    packet.addUInt16(newRegionId);
                    PacketHelper.AddToPacket(sender, packet, true);
                    channel.post(newRegionId, packet);
                }
            }
            else {
                ushort region_id = Region.getRegionIndex(sender);
                regions[region_id].update(sender, sender.X, sender.Y);
                Packet packet = new Packet(Command.OBJECT_UPDATE);
                packet.addUInt16(region_id);
                PacketHelper.AddToPacket(sender, packet, true);
                channel.post(region_id, packet);
            }
        }
        #endregion

        #region Helpers
        public Region getRegion(uint x, uint y) {
            return getRegion((ushort)Region.getRegionIndex(x, y));
        }

        public Region getRegion(ushort id) {
            return regions[id];
        }

        public CityRegion getCityRegion(uint x, uint y) {
            return getCityRegion((ushort)CityRegion.getRegionIndex(x, y));
        }

        public CityRegion getCityRegion(ushort id) {
            return cityRegions[id];
        }
        #endregion

        #region Channel Subscriptions
        internal void subscribeRegion(Session session, ushort id) {
            channel.subscribe(session, id);
        }
        internal void unsubscribeRegion(Session session, ushort id) {
            channel.unsubscribe(session, id);
        }
        internal void unsubscribeRegion(Session session) {
            channel.unsubscribe(session);
        }
        internal void unsubscribeAll(Session session) {
            channel.unsubscribeAll(session);
        }
        #endregion

        internal void lockRegion(uint x, uint y) {
            Monitor.Enter(getRegion(x, y).Lock);
        }

        internal void unlockRegion(uint x, uint y) {
            Monitor.Exit(getRegion(x, y).Lock);
        }

        bool getObjectsForeach(uint ox, uint oy, uint _x, uint _y, object custom) {

            if (_x < WorldWidth && _y < WorldHeight)
                ((List<GameObject>)custom).AddRange(this.getObjects(_x, _y));
            return true;
        }

        public List<GameObject> getObjectsWithin(uint x, uint y, byte radius) {
            List<GameObject> list = new List<GameObject>();
            RadiusLocator.foreach_object(x, y, radius, false, getObjectsForeach, list);
            return list;
        }

        public ushort getTileType(uint x, uint y) {
            Region region = getRegion(x, y);
            return region.getTileType(x, y);
        }

    }
}
