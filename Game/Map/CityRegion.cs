using System;
using System.Collections.Generic;
using System.Text;
using Game.Map;
using Game.Data;
using System.IO;
using System.Collections;


namespace Game {
    public class CityRegion {

        #region Constants
        public const int TILE_SIZE = 2;
        #endregion

        #region Members
        object objLock = new object();
        List<City> data = new List<City>();
        byte[] objects = null;
        bool isDirty = true;

        public List<City> Cities {
            get { return data; }
        }

        public int Count {
            get {
                int count;

                lock (data) {
                    count = data.Count;
                }

                return count;
            }
        }
        #endregion

        #region Methods
        public bool add(City obj) {
            lock (objLock) {
                data.Add(obj);
                isDirty = true;
            }

            return true;
        }

        public void remove(City obj) {
            lock (objLock) {
                data.Remove(obj);
                isDirty = true;
            }
        }
        
        public byte[] getCityBytes() {
            if (isDirty || objects == null) {
                lock (data) {
                    using (MemoryStream ms = new MemoryStream()) {
                        BinaryWriter bw = new BinaryWriter(ms);
                        bw.Write((ushort)Count);
                        foreach (City city in data) {
                            bw.Write((byte)city.MainBuilding.Lvl);
                            bw.Write((ushort)city.MainBuilding.Type);
                            bw.Write((uint)city.MainBuilding.City.Owner.PlayerId);
                            bw.Write((uint)city.MainBuilding.City.CityId);
                            bw.Write((uint)city.MainBuilding.ObjectID);
                            bw.Write((ushort)(city.MainBuilding.CityRegionRelX));
                            bw.Write((ushort)(city.MainBuilding.CityRegionRelY));
                        }

                        isDirty = false;

                        ms.Position = 0;
                        objects = ms.ToArray();
                    }
                }
            }

            return objects;
        }

        #endregion

        #region Static Util Methods
        public static ushort getRegionIndex(City city) {
            return getRegionIndex(city.MainBuilding.X, city.MainBuilding.Y);
        }

        public static ushort getRegionIndex(uint x, uint y) {
            return (ushort)(x / Setup.Config.city_region_width + (y / Setup.Config.city_region_height) * Setup.Config.city_region_column);
        }

        public static int getTileIndex(City city) {
            return getTileIndex(city.MainBuilding.X, city.MainBuilding.Y);
        }

        public static int getTileIndex(uint x, uint y) {
            return (int)(x % Setup.Config.city_region_width + (y % Setup.Config.city_region_height) * Setup.Config.city_region_width);
        }
        #endregion
    }
}
