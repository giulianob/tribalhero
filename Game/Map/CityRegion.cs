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

        public enum ObjectType : byte
        {
            City = 0,
            Forest = 1
        }

        #endregion

        #region Members

        private readonly List<ICityRegionObject> data = new List<ICityRegionObject>();
        private readonly object objLock = new object();
        private bool isDirty = true;
        private byte[] objects;

        #endregion

        #region Methods

        public bool Add(ICityRegionObject obj)
        {
            lock (objLock)
            {
                data.Add(obj);
                isDirty = true;
            }

            return true;
        }

        public void Remove(ICityRegionObject obj)
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
                    if (isDirty || objects == null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            var bw = new BinaryWriter(ms);
                            bw.Write((ushort)data.Count);
                            foreach (var obj in data)
                            {
                                bw.Write((byte)obj.GetCityRegionType());
                                bw.Write(obj.GetCityRegionObjectBytes());
                            }

                            isDirty = false;

                            ms.Position = 0;
                            objects = ms.ToArray();
                        }
                    }
                }
            }

            return objects;
        }

        #endregion

        #region Static Util Methods

        public static ushort GetRegionIndex(uint x, uint y)
        {
            return (ushort)(x/Config.city_region_width + (y/Config.city_region_height)*Config.city_region_column);
        }

        #endregion
    }
}