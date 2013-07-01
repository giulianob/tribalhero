#region

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

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly IActionFactory actionFactory;

        private readonly Dictionary<uint, IForest> forests;

        private readonly LargeIdGenerator objectIdGenerator = new LargeIdGenerator(Config.forest_id_max, Config.forest_id_min);

        private readonly ITileLocator tileLocator;

        public ForestManager(IScheduler scheduler,
                             IWorld world,
                             IDbManager dbManager,
                             Formula formula,
                             IForestFactory forestFactory,
                             IObjectTypeFactory objectTypeFactory,
                             IActionFactory actionFactory,
                             ITileLocator tileLocator)
        {
            this.scheduler = scheduler;
            this.world = world;
            this.dbManager = dbManager;
            this.formula = formula;
            this.forestFactory = forestFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.actionFactory = actionFactory;
            this.tileLocator = tileLocator;

            ForestCount = new int[Config.forest_count.Length];
            forests = new Dictionary<uint, IForest>();
        }

        public int[] ForestCount { get; private set; }
        
        public void StartForestCreator()
        {
            scheduler.Put(actionFactory.CreateForestCreatorAction());
        }

        public void DbLoaderAdd(IForest forest)
        {
            objectIdGenerator.Set(forest.ObjectId);

            ForestCount[forest.Lvl - 1]++;
            forests.Add(forest.ObjectId, forest);
        }

        public bool HasForestNear(uint x, uint y, int radius)
        {
            return world.Regions.GetRegion(x, y)
                        .GetPrimaryObjects()
                        .OfType<IForest>()
                        .Any(forest => tileLocator.TileDistance(forest.X, forest.Y, x, y) <= radius);
        }

        public void CreateForest(byte lvl, int capacity, double rate)
        {
            CreateForestAt(lvl, capacity, rate);
        }

        public void CreateForestAt(byte lvl, int capacity, double rate, uint x = 0, uint y = 0)
        {
            lock (forests)
            {
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
                        var tiles = world.Regions.GetTilesWithin(x, y, 7);
                        if (objectTypeFactory.HasTileType("CityStartTile", tiles))
                        {
                            continue;
                        }

                        var buildtableTiles = world.Regions.GetTilesWithin(x, y, 2);
                        if (!objectTypeFactory.IsAllTileType("TileBuildable", buildtableTiles))
                        {
                            continue;
                        }

                        world.Regions.LockRegion(x, y);

                        // check if near any other objects
                        if (world.Regions.GetObjectsInTile(x, y).Any(obj => !(obj is ITroopObject)) || 
                            world.Regions.GetObjectsWithin(x, y, 1).Any(obj => !(obj is ITroopObject)))
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

                var forest = forestFactory.CreateForest(objectIdGenerator.GetNext(), lvl, capacity, rate, x, y);

                forest.BeginUpdate();
                world.Regions.Add(forest);
                world.Regions.UnlockRegion(x, y);
                forests.Add(forest.ObjectId, forest);                
                forest.RecalculateForest();
                forest.EndUpdate();

                ForestCount[lvl - 1]++;
            }
        }

        public void RemoveForest(IForest forest)
        {
            lock (forests)
            {
                ForestCount[forest.Lvl - 1]--;

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
            for (byte i = 0; i < Config.forest_count.Length; i++)
            {
                var lvl = (byte)(i + 1);
                int delta = Config.forest_count[i] - ForestCount[i];

                for (int j = 0; j < delta; j++)
                {
                    CreateForest(lvl, formula.GetMaxForestCapacity(lvl), formula.GetMaxForestRate(lvl));
                }
            }
        }
    }
}