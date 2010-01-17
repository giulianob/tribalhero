#region

using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Setup;

#endregion

namespace Game {
    public class CityRegion {
        #region Constants

        public const int TILE_SIZE = 2;

        #endregion

        #region Members

        private object objLock = new object();
        private List<City> data = new List<City>();
        private byte[] objects;
        private bool isDirty = true;

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
                        bw.Write((ushort) Count);
                        foreach (City city in data) {
                            bw.Write(city.MainBuilding.Lvl);
                            bw.Write(city.MainBuilding.Type);
                            bw.Write(city.MainBuilding.City.Owner.PlayerId);
                            bw.Write(city.MainBuilding.City.Id);
                            bw.Write(city.MainBuilding.ObjectId);
                            bw.Write((ushort) (city.MainBuilding.CityRegionRelX));
                            bw.Write((ushort) (city.MainBuilding.CityRegionRelY));
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
            return (ushort) (x/Config.city_region_width + (y/Config.city_region_height)*Config.city_region_column);
        }

        public static int getTileIndex(City city) {
            return getTileIndex(city.MainBuilding.X, city.MainBuilding.Y);
        }

        public static int getTileIndex(uint x, uint y) {
            return (int) (x%Config.city_region_width + (y%Config.city_region_height)*Config.city_region_width);
        }

        #endregion
    }
}