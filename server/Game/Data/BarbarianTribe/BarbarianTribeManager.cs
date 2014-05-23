using System;
using System.Collections.Concurrent;
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

        public void Generate(int count)
        {
            for (var i = 0; i < count; ++i)
            {
                byte level;
                Position position;

                if (!barbarianTribeConfigurator.Next(Config.barbariantribe_generate, out level, out position))
                {
                    break;
                }

                IBarbarianTribe barbarianTribe = barbarianTribeFactory.CreateBarbarianTribe(idGenerator.GetNext(), level, position, Config.barbariantribe_camp_count);
                multiObjectLockFactory().Lock(new ILockable[] {barbarianTribe})
                                        .Do(() => Add(barbarianTribe));
                logger.Info(string.Format("Added barbarianTribe[{0},{1}] Number[{2}] ", position.X, position.Y, i));
            }
        }

        public void CreateBarbarianTribeNear(byte level, int campCount, uint x, uint y, byte radius)
        {
            Position barbarianCampPosition;

            int tries = 0;
            do
            {
                tileLocator.RandomPoint(new Position(x, y), radius, true, out barbarianCampPosition);
                if (tries++ > 150)
                {
                    return;
                }
            }
            while (!barbarianTribeConfigurator.IsLocationAvailable(barbarianCampPosition));
            
            IBarbarianTribe barbarianTribe = barbarianTribeFactory.CreateBarbarianTribe(idGenerator.GetNext(), level, barbarianCampPosition, campCount);
            if (barbarianTribe != null)
            {
                Add(barbarianTribe);
            }
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

        private void Remove(IBarbarianTribe barbarianTribe)
        {
            if (barbarianTribe.Battle != null || barbarianTribe.Worker.PassiveActions.Any() || barbarianTribe.Worker.ActiveActions.Any())
            {
                return;
            }            

            IBarbarianTribe obj;
            if (!barbarianTribes.TryRemove(barbarianTribe.ObjectId, out obj))
            {
                return;
            }

            barbarianTribe.CampRemainsChanged -= BarbarianTribeOnCampRemainsChanged;

            if (barbarianTribe.InWorld)
            {
                barbarianTribe.BeginUpdate();
                regionManager.Remove(barbarianTribe);
                barbarianTribe.EndUpdate();
            }

            dbManager.Delete(barbarianTribe);
        }

        public void RelocateAsNeeded()
        {
            var barbarianTribesToDelete = barbarianTribes.Values.Where(bt => (bt.Created.Add(TimeSpan.FromSeconds(Config.barbariantribe_idle_duration_in_sec)) < DateTime.UtcNow &&
                                                                              bt.LastAttacked == DateTime.MinValue) || bt.CampRemains == 0).ToArray();

            foreach(var barbarianTribe in barbarianTribesToDelete)
            {
                multiObjectLockFactory().Lock(new ILockable[] {barbarianTribe})
                                        .Do(() => Remove(barbarianTribe));
            }

            Generate(barbarianTribesToDelete.Count());
        }
    }
}
