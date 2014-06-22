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

        private readonly IActionFactory actionFactory;

        private readonly ILocker locker;

        private readonly Dictionary<uint, IForest> forests;

        private readonly LargeIdGenerator objectIdGenerator = new LargeIdGenerator(Config.forest_id_max, Config.forest_id_min);

        private readonly ITileLocator tileLocator;

        private readonly MapFactory mapFactory;

        public ForestManager(IScheduler scheduler,
                             IWorld world,
                             IDbManager dbManager,
                             Formula formula,
                             IForestFactory forestFactory,
                             IActionFactory actionFactory,
                             ITileLocator tileLocator,
							 ILocker locker,
                             MapFactory mapFactory)
        {
            this.scheduler = scheduler;
            this.world = world;
            this.dbManager = dbManager;
            this.formula = formula;
            this.forestFactory = forestFactory;
			this.actionFactory = actionFactory;
            this.tileLocator = tileLocator;
            this.mapFactory = mapFactory;
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
                        .GetPrimaryObjects()
                        .OfType<IForest>()
                        .Any(forest => tileLocator.TileDistance(forest.PrimaryPosition, 1, new Position(x, y), 1) <= radius);
        }

        public void CreateForest(int capacity)
        {
            CreateForestAt(capacity);
        }

        public void CreateForestAt(int capacity, uint x = 0, uint y = 0)
        {
            lock (forests)
            {
                if (x == 0 || y == 0)
                {
                    while (true)
                    {
                        x = (uint)Config.Random.Next(5, (int)Config.map_width - 5);
                        y = (uint)Config.Random.Next(5, (int)Config.map_height - 5);
                        
                        if (mapFactory.TooCloseToCities(new Position(x, y), 1))
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

                var forest = forestFactory.CreateForest(objectIdGenerator.GetNext(), capacity, x, y);

                forest.BeginUpdate();
                world.Regions.Add(forest);
                world.Regions.UnlockRegion(x, y);
                forests.Add(forest.ObjectId, forest);                
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
                    locker.Lock(CallbackLockHandler, new object[] {kpv.Key}).Do(() =>
                    {
                        kpv.Value.BeginUpdate();
                        kpv.Value.Wood = new AggressiveLazyValue(capacity);
                        kpv.Value.RecalculateForest();
                        kpv.Value.EndUpdate();
                    });
                }
            }
            
        }
    }
}