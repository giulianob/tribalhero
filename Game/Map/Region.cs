#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Game.Comm;
using Game.Data;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Map
{
    public class Region
    {
        #region Constants

        public const int TILE_SIZE = 2;

        #endregion

        #region Members

        private readonly byte[] map;

        private readonly DefaultMultiObjectLock.Factory lockerFactory;

        private readonly ReaderWriterLockSlim objLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly ObjectList objlist = new ObjectList();

        private bool isDirty = true;

        private byte[] objects;

        public ReaderWriterLockSlim Lock
        {
            get
            {
                return objLock;
            }
        }

        #endregion

        #region Constructors

        public Region(byte[] map, DefaultMultiObjectLock.Factory lockerFactory)
        {
            this.map = map;
            this.lockerFactory = lockerFactory;
        }

        #endregion

        #region Methods

        public void Add(ISimpleGameObject obj)
        {
            objLock.EnterWriteLock();
            objlist.AddGameObject(obj);
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public void Remove(ISimpleGameObject obj)
        {
            objLock.EnterWriteLock();
            objlist.Remove(obj);
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public void Remove(ISimpleGameObject obj, uint origX, uint origY)
        {
            objLock.EnterWriteLock();
            objlist.Remove(obj, origX, origY);
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public void Update(ISimpleGameObject obj, uint origX, uint origY)
        {
            objLock.EnterWriteLock();
            if (obj.X != origX || obj.Y != origY)
            {
                objlist.Remove(obj, origX, origY);
                objlist.AddGameObject(obj);
            }
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public void MarkAsDirty()
        {
            objLock.EnterWriteLock();
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public List<ISimpleGameObject> GetObjects(uint x, uint y)
        {
            objLock.EnterReadLock();
            var gameObjects = objlist.Get(x, y);
            objLock.ExitReadLock();
            return gameObjects;
        }

        public IEnumerable<ISimpleGameObject> GetObjects()
        {
            objLock.EnterReadLock();
            var copy = objlist.ToList();            
            objLock.ExitReadLock();

            return copy;
        }

        public byte[] GetObjectBytes()
        {
            while (isDirty)
            {
                // Players must always be locked first
                objLock.EnterReadLock();
                var playersInRegion = objlist.OfType<IGameObject>().Select(p => p.City.Owner).Where(p => p != null).Distinct().ToArray<ILockable>();
                objLock.ExitReadLock();

                using (var lck = lockerFactory().Lock(playersInRegion))
                {
                    // Enter write lock but give up all locks if we cant in 1 second just to be safe
                    if (!objLock.TryEnterWriteLock(1000))
                    {
                        continue;
                    }

                    var lockedPlayersInRegion = objlist.OfType<IGameObject>().Select(p => p.City.Owner).Where(p => p != null).Distinct().ToArray<ILockable>();
                    lck.SortLocks(lockedPlayersInRegion);

                    if (!playersInRegion.SequenceEqual(lockedPlayersInRegion))
                    {
                        objLock.ExitWriteLock();
                        continue;
                    }

                    if (isDirty)
                    {
                        using (var ms = new MemoryStream(map.Length))
                        {
                            var bw = new BinaryWriter(ms);

                            // Write map tiles
                            bw.Write(map);

                            // Write objects
                            bw.Write(objlist.Count);

                            foreach (ISimpleGameObject obj in objlist)
                            {
                                // TODO: Make this not require a packet
                                Packet dummyPacket = new Packet();
                                PacketHelper.AddToPacket(obj, dummyPacket, true);
                                bw.Write(dummyPacket.GetPayload());
                            }

                            ms.Position = 0;
                            objects = ms.ToArray();
                            isDirty = false;
                        }
                    }

                    objLock.ExitWriteLock();
                    break;
                }
            }

            return objects;
        }

        public ushort GetTileType(uint x, uint y)
        {
            objLock.EnterReadLock();
            var tileType = BitConverter.ToUInt16(map, GetTileIndex(x, y) * 2);
            objLock.ExitReadLock();
            return tileType;
        }

        public void SetTileType(uint x, uint y, ushort tileType)
        {
            objLock.EnterWriteLock();
            int idx = GetTileIndex(x, y) * TILE_SIZE;
            byte[] ushortArr = BitConverter.GetBytes(tileType);
            map[idx] = ushortArr[0];
            map[idx + 1] = ushortArr[1];

            isDirty = true;
            objLock.ExitWriteLock();
        }

        #endregion

        #region Static Util Methods

        public static ushort GetRegionIndex(ISimpleGameObject obj)
        {
            return GetRegionIndex(obj.X, obj.Y);
        }

        public static ushort GetRegionIndex(uint x, uint y)
        {
            return (ushort)(x / Config.region_width + (y / Config.region_height) * (int)(Config.map_width / Config.region_width));
        }

        public static int GetTileIndex(uint x, uint y)
        {
            return (int)(x % Config.region_width + (y % Config.region_height) * Config.region_width);
        }

        #endregion
    }
}