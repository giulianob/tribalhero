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

        private readonly RegionObjectList tileObjects;

        private readonly RegionObjectList primaryObjects;

        private bool isDirty = true;

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

        private byte[] objects;

        private int regionLastUpdated;

        public IEnumerable<ISimpleGameObject> GetObjectsInTile(uint x, uint y)
        {
            tileLock.EnterReadLock();
            var copy = tileObjects.Get(x, y).ToArray();
            tileLock.ExitReadLock();
            return copy;
        }

        #endregion

        #region Constructors

        public Region(byte[] map, DefaultMultiObjectLock.Factory lockerFactory, IRegionLocator regionLocator, IRegionObjectListFactory regionObjectListFactory)
        {
            this.map = map;
            this.lockerFactory = lockerFactory;
            this.regionLocator = regionLocator;

            tileObjects = regionObjectListFactory.CreateRegionObjectList();
            primaryObjects = regionObjectListFactory.CreateRegionObjectList();
        }

        public void Add(ISimpleGameObject obj)
        {
            primaryLock.EnterWriteLock();
            primaryObjects.Add(obj, obj.PrimaryPosition.X, obj.PrimaryPosition.Y);
            MarkAsDirty();
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

        private readonly IRegionLocator regionLocator;

        public void Remove(ISimpleGameObject obj)
        {
            primaryLock.EnterWriteLock();
            primaryObjects.Remove(obj, obj.PrimaryPosition.X, obj.PrimaryPosition.Y);
            MarkAsDirty();
            primaryLock.ExitWriteLock();
        }

        public void Remove(ISimpleGameObject obj, uint origX, uint origY)
        {
            primaryLock.EnterWriteLock();
            primaryObjects.Remove(obj, origX, origY);
            MarkAsDirty();
            primaryLock.ExitWriteLock();
        }

        public void MarkAsDirty()
        {
            primaryLock.EnterWriteLock();
            isDirty = true;

            if (regionLastUpdated == int.MaxValue)
            {
                regionLastUpdated = 0;
            }

            regionLastUpdated++;

            primaryLock.ExitWriteLock();
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
                var playersInRegion = primaryObjects.ToArray<ILockable>();
                var currentLastUpdated = regionLastUpdated;
                primaryLock.ExitReadLock();

                var isDone = lockerFactory().Lock(playersInRegion).Do(() =>
                {
                    // Enter write lock but give up all locks if we cant in 1 second just to be safe
                    if (!primaryLock.TryEnterWriteLock(1000))
                    {
                        return false;
                    }

                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                    // ReSharper disable HeuristicUnreachableCode
                    if (!isDirty)
                    {
                        primaryLock.ExitWriteLock();
                        return true;
                    }
                    // ReSharper restore HeuristicUnreachableCode
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse

                    if (currentLastUpdated != regionLastUpdated)
                    {
                        primaryLock.ExitWriteLock();
                        return false;
                    }

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

                    primaryLock.ExitWriteLock();

                    return true;
                });

                if (isDone)
                {
                    break;
                }
            }

            return objects;
        }

        public ushort GetTileType(uint x, uint y)
        {
            primaryLock.EnterReadLock();
            var tileType = BitConverter.ToUInt16(map, regionLocator.GetTileIndex(x, y) * 2);
            primaryLock.ExitReadLock();
            return tileType;
        }

        public void SetTileType(uint x, uint y, ushort tileType)
        {
            primaryLock.EnterWriteLock();

            int idx = regionLocator.GetTileIndex(x, y) * TILE_SIZE;
            byte[] ushortArr = BitConverter.GetBytes(tileType);
            Array.Copy(ushortArr, 0, map, idx, ushortArr.Length);

            MarkAsDirty();
            primaryLock.ExitWriteLock();
        }

        #endregion
    }
}