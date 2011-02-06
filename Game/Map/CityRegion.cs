#region

using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Map
{
    public class CityRegion
    {
        #region Constants

        public const int TILE_SIZE = 2;

        #endregion

        #region Members

        private readonly List<City> data = new List<City>();
        private readonly object objLock = new object();
        private bool isDirty = true;
        private byte[] objects;

        public List<City> Cities
        {
            get
            {
                return data;
            }
        }

        public int Count
        {
            get
            {
                int count;

                lock (data)
                {
                    count = data.Count;
                }

                return count;
            }
        }

        #endregion

        #region Methods

        public bool Add(City obj)
        {
            lock (objLock)
            {
                data.Add(obj);
                isDirty = true;
            }

            return true;
        }

        public void Remove(City obj)
        {
            lock (objLock)
            {
                data.Remove(obj);
                isDirty = true;
            }
        }

        public byte[] GetCityBytes()
        {
            if (isDirty || objects == null)
            {
                lock (data)
                {
                    using (var ms = new MemoryStream())
                    {
                        var bw = new BinaryWriter(ms);
                        bw.Write((ushort)Count);
                        foreach (var city in data)
                        {
                            bw.Write(city.MainBuilding.Lvl);
                            bw.Write(city.MainBuilding.Type);
                            bw.Write(city.MainBuilding.City.Owner.PlayerId);
                            bw.Write(city.MainBuilding.City.Id);
                            bw.Write(city.MainBuilding.ObjectId);
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

        public static ushort GetRegionIndex(City city)
        {
            return GetRegionIndex(city.MainBuilding.X, city.MainBuilding.Y);
        }

        public static ushort GetRegionIndex(uint x, uint y)
        {
            return (ushort)(x/Config.city_region_width + (y/Config.city_region_height)*Config.city_region_column);
        }

        public static int GetTileIndex(City city)
        {
            return GetTileIndex(city.MainBuilding.X, city.MainBuilding.Y);
        }

        public static int GetTileIndex(uint x, uint y)
        {
            return (int)(x%Config.city_region_width + (y%Config.city_region_height)*Config.city_region_width);
        }

        #endregion
    }
}