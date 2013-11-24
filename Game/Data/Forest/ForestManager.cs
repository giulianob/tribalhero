﻿#region

using System.Collections.Generic;
using System.Linq;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Data.Forest
{
    public class ForestManager : IForestManager
    {
        private readonly IScheduler scheduler;

        private readonly IWorld world;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly IForestFactory forestFactory;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly IActionFactory actionFactory;

        private readonly ILocker locker;

        private readonly Dictionary<uint, IForest> forests;

        private readonly LargeIdGenerator objectIdGenerator = new LargeIdGenerator(Config.forest_id_max, Config.forest_id_min);

        public ForestManager(IScheduler scheduler,
                             IWorld world,
                             IDbManager dbManager,
                             Formula formula,
                             IForestFactory forestFactory,
                             ObjectTypeFactory objectTypeFactory,
                             IActionFactory actionFactory,
                             ILocker locker)
        {
            this.scheduler = scheduler;
            this.world = world;
            this.dbManager = dbManager;
            this.formula = formula;
            this.forestFactory = forestFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.actionFactory = actionFactory;
            this.locker = locker;
            forests = new Dictionary<uint, IForest>();
        }

        public int ForestCount
        {
            get
            {
                return forests.Count;
            }
        }

        public void StartForestCreator()
        {
            scheduler.Put(actionFactory.CreateForestCreatorAction());
        }

        public void DbLoaderAdd(IForest forest)
        {
            objectIdGenerator.Set(forest.ObjectId);

            forests.Add(forest.ObjectId, forest);
        }

        public bool HasForestNear(uint x, uint y, int radius)
        {
            return world.Regions.GetRegion(x, y)
                        .GetObjects()
                        .OfType<IForest>()
                        .Any(forest => forest.TileDistance(x, y) <= radius);
        }

        public void CreateForest(int capacity)
        {
            CreateForestAt(capacity);
        }

        public void CreateForestAt(int capacity, uint x = 0, uint y = 0)
        {
            lock (forests)
            {
                var forest = forestFactory.CreateForest(capacity);

                if (x == 0 || y == 0)
                {
                    while (true)
                    {
                        x = (uint)Config.Random.Next(5, (int)Config.map_width - 5);
                        y = (uint)Config.Random.Next(5, (int)Config.map_height - 5);

                        if (!objectTypeFactory.IsTileType("TileBuildable", world.Regions.GetTileType(x, y)))
                        {
                            continue;
                        }

                        // check if tile is safe
                        List<ushort> tiles = world.Regions.GetTilesWithin(x, y, 9);
                        if (objectTypeFactory.HasTileType("CityStartTile", tiles))
                        {
                            continue;
                        }

                        List<ushort> buildtableTiles = world.Regions.GetTilesWithin(x, y, 2);
                        if (!objectTypeFactory.IsAllTileType("TileBuildable", buildtableTiles))
                        {
                            continue;
                        }

                        world.Regions.LockRegion(x, y);

                        // check if near any other objects
                        if (world.GetObjects(x, y).Exists(obj => !(obj is ITroopObject)) || world.GetObjectsWithin(x, y, 1).Exists(obj => !(obj is ITroopObject)))
                        {
                            world.Regions.UnlockRegion(x, y);
                            continue;
                        }

                        break;
                    }
                }
                else
                {
                    world.Regions.LockRegion(x, y);
                }

                forest.X = x;
                forest.Y = y;

                forest.ObjectId = objectIdGenerator.GetNext();

                world.Regions.Add(forest);
                world.Regions.UnlockRegion(x, y);

                forests.Add(forest.ObjectId, forest);

                forest.BeginUpdate();
                forest.RecalculateForest();
                forest.EndUpdate();

            }
        }

        public void RemoveForest(IForest forest)
        {
            lock (forests)
            {
                forests.Remove(forest.ObjectId);

                forest.BeginUpdate();
                world.Regions.Remove(forest);
                forest.EndUpdate();

                dbManager.Delete(forest);
            }
        }

        /// <summary>
        ///     Locks all cities participating in this forest.
        ///     Once inside of the lock, a call to ForestManager.TryGetStronghold should be used to get the forest.
        /// </summary>
        /// <param name="custom">custom[0] should contain the forestId to lock</param>
        /// <returns>List of cities to lock for the forest.</returns>
        public ILockable[] CallbackLockHandler(object[] custom)
        {
            return GetListOfLocks((uint)custom[0]);
        }

        private ILockable[] GetListOfLocks(uint forestId)
        {
            lock (forests)
            {
                IForest forest;

                return !forests.TryGetValue(forestId, out forest)
                               ? new ILockable[] {}
                               : forest.Select(obj => obj.City).Concat(new ILockable[] {forest}).ToArray();
            }
        }

        public bool TryGetValue(uint id, out IForest forest)
        {
            lock (forests)
            {
                return forests.TryGetValue(id, out forest);
            }
        }

        public void RegenerateForests()
        {
            int delta = Config.forest_count - forests.Count;

            for (int j = 0; j < delta; j++)
            {
                CreateForest(formula.GetMaxForestCapacity());
            }
        }

        public void ReloadForests(int capacity)
        {
            lock (forests)
            {
                foreach (var kpv in forests)
                {
                    using (locker.Lock(CallbackLockHandler, new object[] {kpv.Key}))
                    {
                        kpv.Value.BeginUpdate();
                        kpv.Value.Wood = new AggressiveLazyValue(capacity);
                        kpv.Value.RecalculateForest();
                        kpv.Value.EndUpdate();
                    }
                }
            }
            
        }
    }
}