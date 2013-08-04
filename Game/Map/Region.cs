#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Game.Comm;
using Game.Data;
using Game.Util.Locking;

#endregion

namespace Game.Map
{
    public class Region : IRegion
    {
        #region Constants

        public const int TILE_SIZE = sizeof(ushort);

        #endregion

        #region Members

        private readonly byte[] map;

        private readonly DefaultMultiObjectLock.Factory lockerFactory;

        private readonly ReaderWriterLockSlim primaryLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly ReaderWriterLockSlim tileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly RegionObjectList tileObjects = new RegionObjectList();

        private readonly RegionObjectList primaryObjects = new RegionObjectList();

        private bool isDirty = true;

        private byte[] objects;

        #endregion

        #region Methods

        public void AddObjectToTile(ISimpleGameObject obj, uint x, uint y)
        {
            tileLock.EnterWriteLock();
            tileObjects.Add(obj, x, y);
            tileLock.ExitWriteLock();
        }

        public void RemoveObjectFromTile(ISimpleGameObject obj, uint x, uint y)
        {
            tileLock.EnterWriteLock();
            tileObjects.Remove(obj, x, y);
            tileLock.ExitWriteLock();
        }

        #endregion

        #region Constructors

        public Region(byte[] map, DefaultMultiObjectLock.Factory lockerFactory)
        {
            this.map = map;
            this.lockerFactory = lockerFactory;
        }

        public void Add(ISimpleGameObject obj)
        {
            primaryLock.EnterWriteLock();
            primaryObjects.Add(obj, obj.PrimaryPosition.X, obj.PrimaryPosition.Y);
            isDirty = true;
            primaryLock.ExitWriteLock();
        }

        public void Remove(ISimpleGameObject obj)
        {
            primaryLock.EnterWriteLock();
            primaryObjects.Remove(obj, obj.PrimaryPosition.X, obj.PrimaryPosition.Y);
            isDirty = true;
            primaryLock.ExitWriteLock();
        }

        public IEnumerable<ISimpleGameObject> GetObjectsInTile(uint x, uint y)
        {
            tileLock.EnterReadLock();
            var copy = tileObjects.Get(x, y).ToArray();
            tileLock.ExitReadLock();
            return copy;
        }

        public void Remove(ISimpleGameObject obj, uint origX, uint origY)
        {
            primaryLock.EnterWriteLock();
            primaryObjects.Remove(obj, origX, origY);
            isDirty = true;
            primaryLock.ExitWriteLock();
        }

        public void MarkAsDirty()
        {
            primaryLock.EnterWriteLock();
            isDirty = true;
            primaryLock.ExitWriteLock();
        }

        public void EnterWriteLock()
        {
            primaryLock.EnterWriteLock();
        }

        public void ExitWriteLock()
        {
            primaryLock.ExitWriteLock();
        }

        #endregion

        #region Static Util Methods

        private static IRegionLocator regionLocator = new RegionLocator();

        public static IRegionLocator RegionLocator
        {
            get
            {
                return regionLocator;
            }
            set
            {
                regionLocator = value;
            }
        }

        public IEnumerable<ISimpleGameObject> GetPrimaryObjects()
        {
            primaryLock.EnterReadLock();
            var copy = primaryObjects.ToArray();            
            primaryLock.ExitReadLock();
            return copy;
        }

        public byte[] GetObjectBytes()
        {
            while (isDirty)
            {
                // Players must always be locked first
                primaryLock.EnterReadLock();
                var playersInRegion = primaryObjects.OfType<IGameObject>()
                                             .Select(p => p.City.Owner)
                                             .Where(p => p != null)
                                             .Distinct(new LockableComparer())
                                             .ToArray();
                primaryLock.ExitReadLock();

                using (var lck = lockerFactory().Lock(playersInRegion))
                {
                    // Enter write lock but give up all locks if we cant in 1 second just to be safe
                    if (!primaryLock.TryEnterWriteLock(1000))
                    {
                        continue;
                    }

                    var lockedPlayersInRegion = primaryObjects.OfType<IGameObject>()
                                                       .Select(p => p.City.Owner)
                                                       .Where(p => p != null)
                                                       .Distinct(new LockableComparer())
                                                       .ToArray();

                    lck.SortLocks(lockedPlayersInRegion);
                    lck.SortLocks(playersInRegion);

                    if (!playersInRegion.SequenceEqual(lockedPlayersInRegion, new LockableComparer()))
                    {
                        primaryLock.ExitWriteLock();
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
                            bw.Write(primaryObjects.Count);

                            foreach (ISimpleGameObject obj in primaryObjects)
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

                    primaryLock.ExitWriteLock();
                    break;
                }
            }

            return objects;
        }

        public ushort GetTileType(uint x, uint y)
        {
            primaryLock.EnterReadLock();
            var tileType = BitConverter.ToUInt16(map, GetTileIndex(x, y) * 2);
            primaryLock.ExitReadLock();
            return tileType;
        }

        public void SetTileType(uint x, uint y, ushort tileType)
        {
            primaryLock.EnterWriteLock();

            int idx = GetTileIndex(x, y) * TILE_SIZE;
            byte[] ushortArr = BitConverter.GetBytes(tileType);
            Array.Copy(ushortArr, 0, map, idx, ushortArr.Length);

            isDirty = true;
            primaryLock.ExitWriteLock();
        }

        public static ushort GetRegionIndex(ISimpleGameObject obj)
        {
            return regionLocator.GetRegionIndex(obj.PrimaryPosition.X, obj.PrimaryPosition.Y);
        }

        public static ushort GetRegionIndex(uint x, uint y)
        {
            return regionLocator.GetRegionIndex(x, y);
        }

        public static int GetTileIndex(uint x, uint y)
        {
            return regionLocator.GetTileIndex(x, y);
        }

        #endregion
    }
}