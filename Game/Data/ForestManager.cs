#region

using System.Collections.Generic;
using System.Linq;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions.ResourceActions;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Data
{
    public class ForestManager : ILockable
    {
        public static readonly object ForestLock = new object();

        private readonly Dictionary<uint, Forest> forests;

        private readonly LargeIdGenerator objectIdGenerator = new LargeIdGenerator(int.MaxValue);

        public ForestManager()
        {
            ForestCount = new int[Config.forest_count.Length];
            forests = new Dictionary<uint, Forest>();
        }

        public int[] ForestCount { get; private set; }

        #region ILockable Members

        public int Hash
        {
            get
            {
                return (int)Global.Locks.Forest;
            }
        }

        public object Lock
        {
            get
            {
                return ForestLock;
            }
        }

        #endregion

        public void StartForestCreator()
        {
            Scheduler.Current.Put(new ForestCreatorAction());
        }

        public void DbLoaderAdd(Forest forest)
        {
            objectIdGenerator.Set(forest.ObjectId);

            ForestCount[forest.Lvl - 1]++;
            forests.Add(forest.ObjectId, forest);
        }

        public static bool HasForestNear(uint x, uint y, int radius)
        {
            return
                    World.Current.Regions.GetRegion(x, y)
                         .GetObjects()
                         .Any(forest => forest is Forest && forest.TileDistance(x, y) <= radius);
        }

        public void CreateForest(byte lvl, int capacity, double rate)
        {
            CreateForestAt(lvl, capacity, rate, 0, 0);
        }

        public void CreateForestAt(byte lvl, int capacity, double rate, uint x, uint y)
        {
            lock (forests)
            {
                var forest = new Forest(lvl, capacity, rate);

                if (x == 0 || y == 0)
                {
                    while (true)
                    {
                        x = (uint)Config.Random.Next(15, (int)Config.map_width - 15);
                        y = (uint)Config.Random.Next(15, (int)Config.map_height - 15);

                        if (
                                !Ioc.Kernel.Get<ObjectTypeFactory>()
                                    .IsTileType("TileBuildable", World.Current.Regions.GetTileType(x, y)))
                        {
                            continue;
                        }

                        // check if tile is safe
                        List<ushort> tiles = World.Current.Regions.GetTilesWithin(x, y, 7);
                        if (Ioc.Kernel.Get<ObjectTypeFactory>().HasTileType("CityStartTile", tiles))
                        {
                            continue;
                        }

                        if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsAllTileType("TileBuildable", tiles))
                        {
                            continue;
                        }

                        World.Current.Regions.LockRegion(x, y);

                        // check if near any other objects
                        if (World.Current.GetObjects(x, y).Exists(obj => !(obj is ITroopObject)) ||
                            World.Current.GetObjectsWithin(x, y, 4).Exists(obj => !(obj is ITroopObject)))
                        {
                            World.Current.Regions.UnlockRegion(x, y);
                            continue;
                        }

                        break;
                    }
                }
                else
                {
                    World.Current.Regions.LockRegion(x, y);
                }

                forest.X = x;
                forest.Y = y;

                forest.ObjectId = (uint)objectIdGenerator.GetNext();

                World.Current.Regions.Add(forest);
                World.Current.Regions.UnlockRegion(x, y);

                forests.Add(forest.ObjectId, forest);

                forest.BeginUpdate();
                forest.RecalculateForest();
                forest.EndUpdate();

                ForestCount[lvl - 1]++;
            }
        }

        public void RemoveForest(Forest forest)
        {
            lock (forests)
            {
                ForestCount[forest.Lvl - 1]--;

                forests.Remove(forest.ObjectId);

                forest.BeginUpdate();
                World.Current.Regions.Remove(forest);
                forest.EndUpdate();

                DbPersistance.Current.Delete(forest);
            }
        }

        /// <summary>
        ///     Locks all cities participating in this forest.
        ///     Proper usage would be to lock the forest manager and the main city in the base objects.
        ///     The custom[0] parameter should a uint with the forestId.
        ///     Once inside of the lock, a call to ForestManager.TryGetStronghold should be used to get the forest.
        /// </summary>
        /// <param name="custom">custom[0] should contain the forestId to lock</param>
        /// <returns>List of cities to lock for the forest.</returns>
        public ILockable[] CallbackLockHandler(object[] custom)
        {
            return GetListOfLocks((uint)custom[0]);
        }

        public ILockable[] GetListOfLocks(uint forestId)
        {
            lock (forests)
            {
                DefaultMultiObjectLock.ThrowExceptionIfNotLocked(World.Current.Forests);

                Forest forest;

                return !forests.TryGetValue(forestId, out forest)
                               ? new ILockable[] {}
                               : forest.Select(obj => obj.City).ToArray<ILockable>();
            }
        }

        public bool TryGetValue(uint id, out Forest forest)
        {
            lock (forests)
            {
                return forests.TryGetValue(id, out forest);
            }
        }
    }
}