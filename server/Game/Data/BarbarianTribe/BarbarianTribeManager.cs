using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Common;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;
using System.Linq;

namespace Game.Data.BarbarianTribe
{
    public class BarbarianTribeManager : IBarbarianTribeManager
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<BarbarianTribeManager>();
        private readonly IDbManager dbManager;        
        private readonly IBarbarianTribeFactory barbarianTribeFactory;
        private readonly IBarbarianTribeConfigurator barbarianTribeConfigurator;
        private readonly IRegionManager regionManager;

        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory;

        private readonly ITileLocator tileLocator;

        private readonly ConcurrentDictionary<uint, IBarbarianTribe> barbarianTribes = new ConcurrentDictionary<uint, IBarbarianTribe>();
        private readonly LargeIdGenerator idGenerator = new LargeIdGenerator(Config.barbariantribe_id_max, Config.barbariantribe_id_min);

        public BarbarianTribeManager(IDbManager dbManager,                                     
                                     IBarbarianTribeFactory barbarianTribeFactory,
                                     IBarbarianTribeConfigurator barbarianTribeConfigurator,
                                     IRegionManager regionManager,
                                     DefaultMultiObjectLock.Factory multiObjectLockFactory,
                                     ITileLocator tileLocator)
        {
            this.dbManager = dbManager;            
            this.barbarianTribeFactory = barbarianTribeFactory;
            this.barbarianTribeConfigurator = barbarianTribeConfigurator;
            this.regionManager = regionManager;
            this.multiObjectLockFactory = multiObjectLockFactory;
            this.tileLocator = tileLocator;
        }

        public int Count
        {
            get
            {
                return barbarianTribes.Count;
            }
        }

        public void DbLoaderAdd(IBarbarianTribe barbarianTribe)
        {
            idGenerator.Set(barbarianTribe.ObjectId);

            barbarianTribes.AddOrUpdate(barbarianTribe.ObjectId, barbarianTribe, (id, old) => barbarianTribe);            
            regionManager.DbLoaderAdd(barbarianTribe);
            barbarianTribe.CampRemainsChanged += BarbarianTribeOnCampRemainsChanged;
        }

        public bool TryGetBarbarianTribe(uint id, out IBarbarianTribe barbarianTribe)
        {
            return barbarianTribes.TryGetValue(id, out barbarianTribe);
        }

        public void Generate()
        {
            var sw = new Stopwatch();
            sw.Start();

            foreach (var newBarbarianTribeConfiguration in barbarianTribeConfigurator.Create(AllTribes, Count))
            {
                var newBarbarianTribe = barbarianTribeFactory.CreateBarbarianTribe(idGenerator.GetNext(),
                                                                                   newBarbarianTribeConfiguration.Level,
                                                                                   newBarbarianTribeConfiguration.PrimaryPosition,
                                                                                   Config.barbariantribe_camp_count);

                multiObjectLockFactory().Lock(new ILockable[] {newBarbarianTribe})
                                        .Do(() => Add(newBarbarianTribe));

                logger.Info(string.Format("Added barbarianTribe[{0},{1}] Number[{2}] ",
                                          newBarbarianTribe.PrimaryPosition.X,
                                          newBarbarianTribe.PrimaryPosition.Y,
                                          Count));
            }

            sw.Stop();
            logger.Info("Completed generating barb camps in {0}", sw.Elapsed);
        }

        public bool CreateBarbarianTribeNear(byte level, int campCount, uint x, uint y, byte radius)
        {
            Position barbarianCampPosition;

            int tries = 0;
            do
            {
                tileLocator.RandomPoint(new Position(x, y), radius, true, out barbarianCampPosition);
                
                if (barbarianTribeConfigurator.IsLocationAvailable(barbarianCampPosition))
                {
                    break;
                }
                
                if (tries++ > 150)
                {
                    return false;
                }

                if (tries > 0 && radius == 0)
                {
                    return false;
                }
            }
            while (true);
            
            IBarbarianTribe barbarianTribe = barbarianTribeFactory.CreateBarbarianTribe(idGenerator.GetNext(), level, barbarianCampPosition, campCount);
            if (barbarianTribe != null)
            {
                Add(barbarianTribe);
            }

            return true;
        }

        private void Add(IBarbarianTribe barbarianTribe)
        {
            barbarianTribes.AddOrUpdate(barbarianTribe.ObjectId, barbarianTribe, (id, old) => barbarianTribe);

            barbarianTribe.BeginUpdate();
            regionManager.Add(barbarianTribe);
            barbarianTribe.EndUpdate();

            barbarianTribe.CampRemainsChanged += BarbarianTribeOnCampRemainsChanged;
        }

        private void BarbarianTribeOnCampRemainsChanged(object sender, EventArgs eventArgs)
        {
            var barbarianTribe = (IBarbarianTribe)sender;
            if (barbarianTribe.CampRemains == 0 && barbarianTribe.InWorld)
            {
                regionManager.Remove(barbarianTribe);
            }
        }

        public bool RelocateBarbarianTribe(IBarbarianTribe barbarianTribe)
        {
            return multiObjectLockFactory().Lock(new ILockable[] {barbarianTribe})
                                           .Do(() => Remove(barbarianTribe));
        }

        private bool Remove(IBarbarianTribe barbarianTribe)
        {
            if (barbarianTribe.Battle != null || barbarianTribe.Worker.PassiveActions.Any() || barbarianTribe.Worker.ActiveActions.Any())
            {
                return false;
            }            

            IBarbarianTribe obj;
            if (!barbarianTribes.TryRemove(barbarianTribe.ObjectId, out obj))
            {
                return false;
            }

            barbarianTribe.CampRemainsChanged -= BarbarianTribeOnCampRemainsChanged;

            if (barbarianTribe.InWorld)
            {
                barbarianTribe.BeginUpdate();
                regionManager.Remove(barbarianTribe);
                barbarianTribe.EndUpdate();
            }

            dbManager.Delete(barbarianTribe);

            return true;
        }

        public void RelocateAsNeeded()
        {
            var minIdleTime = TimeSpan.FromSeconds(Config.barbariantribe_idle_duration_in_sec);

            var barbarianTribesToDelete = barbarianTribes.Values.Where(bt => (bt.Created.Add(minIdleTime) < DateTime.UtcNow && bt.LastAttacked.Add(minIdleTime) < DateTime.UtcNow) ||
                                                                             bt.CampRemains == 0)
                                                         .ToArray();

            foreach(var barbarianTribe in barbarianTribesToDelete)
            {
                multiObjectLockFactory().Lock(new ILockable[] {barbarianTribe})
                                        .Do(() => Remove(barbarianTribe));
            }

            Generate();
        }

        public void RelocateIdleBarbCamps()
        {
            var barbarianTribesToDelete = barbarianTribes.Values.Where(bt => bt.LastAttacked == DateTime.MinValue).ToArray();

            foreach(var barbarianTribe in barbarianTribesToDelete)
            {
                multiObjectLockFactory().Lock(new ILockable[] {barbarianTribe})
                                        .Do(() => Remove(barbarianTribe));
            }

            Generate();
        }

        public IEnumerable<IBarbarianTribe> AllTribes
        {
            get
            {
                return this.barbarianTribes.Values.AsEnumerable();
            }
        }
    }
}
