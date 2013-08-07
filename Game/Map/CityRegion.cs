#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Map
{
    public class CityRegion
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly DefaultMultiObjectLock.Factory lockerFactory;

        private readonly IGlobal global;

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

        private int regionLastUpdated;

        #endregion

        public CityRegion(DefaultMultiObjectLock.Factory lockerFactory, IGlobal global)
        {
            this.lockerFactory = lockerFactory;
            this.global = global;
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
                MarkAsDirty();
            }

            if (global.FireEvents)
            {
                logger.Info("Added city region obj: {0}", obj.ToString());
            }

            return true;
        }

        public void Remove(ICityRegionObject obj)
        {
            lock (objLock)
            {
                var remove = data.Remove(obj);
                if (!remove)
                {
                    logger.Warn("Tried to remove nonexistant object from city region: {0}", obj.ToString());

                    throw new Exception("Tried to remove obj from wrong region");
                }
                
                logger.Info("Removed city region obj: {0}", obj.ToString());
                
                MarkAsDirty();
            }
        }

        public void MarkAsDirty()
        {
            isDirty = true;

            if (regionLastUpdated == int.MaxValue)
            {
                regionLastUpdated = 0;
            }

            regionLastUpdated++;            
        }

        public void Update(ICityRegionObject obj, uint origX, uint origY)
        {
            lock (objLock)
            {
                Position loc = obj.PrimaryPosition;
                if (loc.X != origX || loc.Y != origY)
                {
                    Remove(obj);
                    Add(obj);
                }

                MarkAsDirty();
            }
        }

        public byte[] GetCityBytes()
        {

            while (isDirty)
            {
                // Players must always be locked first
                ILockable[] playersInRegion;
                decimal currentUpdatedVersion;
                lock (objLock)
                {
                    playersInRegion = data.ToArray<ILockable>();
                    currentUpdatedVersion = regionLastUpdated;
                }

                using (lockerFactory().Lock(playersInRegion))
                {
                    lock (objLock)
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        // ReSharper disable HeuristicUnreachableCode
                        if (!isDirty)                                                        
                        {
                            break;
                        }
                        // ReSharper restore HeuristicUnreachableCode

                        if (currentUpdatedVersion != regionLastUpdated)
                        {
                            continue;
                        }

                        using (var ms = new MemoryStream())
                        {
                            var bw = new BinaryWriter(ms);
                            bw.Write((ushort)data.Count);
                            foreach (var obj in data)
                            {
                                    // TODO: Remove this at some point.. added this to check an existing issue
                                    var simpleObj = obj as ISimpleGameObject;
                                    if (simpleObj != null && !simpleObj.InWorld)
                                    {
                                        logger.Warn("Tried to get bytes from an obj that is not in world {0}", simpleObj.ToString());
                                        throw new Exception("Object not being removed properly...");
                                    }

                                bw.Write((byte)obj.CityRegionType);
                                bw.Write((ushort)(obj.PrimaryPosition.X % Config.city_region_width));
                                bw.Write((ushort)(obj.PrimaryPosition.Y % Config.city_region_height));
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