using System;
using System.Collections.Concurrent;
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
        private readonly IDbManager dbManager;        
        private readonly IBarbarianTribeFactory barbarianTribeFactory;
        private readonly IBarbarianTribeConfigurator barbarianTribeConfigurator;
        private readonly IRegionManager regionManager;

        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory;

        private readonly ConcurrentDictionary<uint, IBarbarianTribe> barbarianTribes = new ConcurrentDictionary<uint, IBarbarianTribe>();
        private readonly LargeIdGenerator idGenerator = new LargeIdGenerator(50000, 10000);

        public BarbarianTribeManager(IDbManager dbManager,                                     
                                     IBarbarianTribeFactory barbarianTribeFactory,
                                     IBarbarianTribeConfigurator barbarianTribeConfigurator,
                                     IRegionManager regionManager,
                                     DefaultMultiObjectLock.Factory multiObjectLockFactory)
        {
            this.dbManager = dbManager;            
            this.barbarianTribeFactory = barbarianTribeFactory;
            this.barbarianTribeConfigurator = barbarianTribeConfigurator;
            this.regionManager = regionManager;
            this.multiObjectLockFactory = multiObjectLockFactory;
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
            idGenerator.Set(barbarianTribe.Id);

            barbarianTribes.AddOrUpdate(barbarianTribe.Id, barbarianTribe, (id, old) => barbarianTribe);            
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
                uint x, y;

                if (!barbarianTribeConfigurator.Next(Config.barbariantribe_generate, out level, out x, out y))
                {
                    break;
                }

                IBarbarianTribe barbarianTribe = barbarianTribeFactory.CreateBarbarianTribe((uint)idGenerator.GetNext(), level, x, y, Config.barbariantribe_camp_count);
                Add(barbarianTribe);
            }
        }

        private void Add(IBarbarianTribe barbarianTribe)
        {
            barbarianTribes.AddOrUpdate(barbarianTribe.Id, barbarianTribe, (id, old) => barbarianTribe);

            using (multiObjectLockFactory().Lock(new ILockable[] {barbarianTribe}))
            {
                barbarianTribe.BeginUpdate();
                barbarianTribe.State = GameObjectState.NormalState();
                regionManager.Add(barbarianTribe);
                barbarianTribe.EndUpdate();

                barbarianTribe.CampRemainsChanged += BarbarianTribeOnCampRemainsChanged;
            }            
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
            if (!barbarianTribes.TryRemove(barbarianTribe.Id, out obj))
            {
                return;
            }

            barbarianTribe.CampRemainsChanged -= BarbarianTribeOnCampRemainsChanged;

            barbarianTribe.BeginUpdate();
            regionManager.Remove(barbarianTribe);
            barbarianTribe.EndUpdate();

            dbManager.Delete(barbarianTribe);
        }

        public void RelocateAsNeeded()
        {
            var barbarianTribesToDelete =
                    barbarianTribes.Values.Where(
                                                 bt =>
                                                 (bt.Created.Add(TimeSpan.FromSeconds(Config.barbariantribe_idle_duration_in_sec)) < DateTime.UtcNow &&
                                                  bt.LastAttacked == DateTime.MinValue) || bt.CampRemains == 0);

            foreach(var barbarianTribe in barbarianTribesToDelete)
            {
                using (multiObjectLockFactory().Lock(new ILockable[] {barbarianTribe}))
                {
                    Remove(barbarianTribe);
                }
            }

            Generate(Math.Max(0, Config.barbariantribe_generate - barbarianTribes.Count));
        }
    }
}
