#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Data;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Map
{
    public class CityRegion
    {
        private readonly DefaultMultiObjectLock.Factory lockerFactory;

        #region Constants

        public enum ObjectType : byte
        {
            City = 0,

            Forest = 1,

            Troop = 2,

            Stronghold = 6,

            BarbarianTribe = 7
        }

        public const int TILE_SIZE = 2;

        #endregion

        public CityRegion(DefaultMultiObjectLock.Factory lockerFactory)
        {
            this.lockerFactory = lockerFactory;
        }

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

        public void MarkAsDirty()
        {
            isDirty = true;
        }

        public void Update(ICityRegionObject obj, uint origX, uint origY)
        {
            lock (objLock)
            {
                Position loc = obj.CityRegionLocation;
                if (loc.X != origX || loc.Y != origY)
                {
                    Remove(obj);
                    Add(obj);
                }
                isDirty = true;
            }
        }

        public byte[] GetCityBytes()
        {

            while (isDirty)
            {
                // Players must always be locked first
                ILockable[] playersInRegion;
                lock (objLock)
                {
                    playersInRegion = data.ToArray<ILockable>();
                }

                using (var lck = lockerFactory().Lock(playersInRegion))
                {
                    // Enter write lock but give up all locks if we cant in 1 second just to be safe
                    lock (objLock)
                    {
                        var lockedPlayersInRegion = data.ToArray<ILockable>();
                        lck.SortLocks(lockedPlayersInRegion);

                        if (!playersInRegion.SequenceEqual(lockedPlayersInRegion))
                        {                            
                            continue;
                        }

                        if (isDirty)
                        {
                            using (var ms = new MemoryStream())
                            {
                                var bw = new BinaryWriter(ms);
                                bw.Write((ushort)data.Count);
                                foreach (var obj in data)
                                {
                                    bw.Write((byte)obj.CityRegionType);
                                    bw.Write(obj.CityRegionRelX);
                                    bw.Write(obj.CityRegionRelY);
                                    bw.Write(obj.CityRegionGroupId);
                                    bw.Write(obj.CityRegionObjectId);

                                    bw.Write(obj.GetCityRegionObjectBytes());
                                }

                                isDirty = false;

                                ms.Position = 0;
                                objects = ms.ToArray();
                            }
                        }

                        break;
                    }                    
                }
            }

            return objects;
        }

        #endregion

        #region Static Util Methods

        public static ushort GetRegionIndex(uint x, uint y)
        {
            return (ushort)(x / Config.city_region_width + (y / Config.city_region_height) * (int)(Config.map_width / Config.city_region_width));
        }

        #endregion
    }
}