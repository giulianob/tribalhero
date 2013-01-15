using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.BarbarianTribe
{
    public class BarbarianTribeManager : IBarbarianTribeManager
    {
        private readonly IDbManager dbManager;
        private readonly SimpleStubGenerator simpleStubGenerator;
        private readonly IBarbarianTribeFactory barbarianTribeFactory;
        private readonly IBarbarianTribeConfigurator barbarianTribeConfigurator;
        private readonly IRegionManager regionManager;

        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory;

        private readonly ConcurrentDictionary<uint, IBarbarianTribe> barbarianTribes = new ConcurrentDictionary<uint, IBarbarianTribe>();
        private readonly LargeIdGenerator idGenerator = new LargeIdGenerator(10000, 5000);

        public BarbarianTribeManager(IDbManager dbManager,
                                     SimpleStubGenerator simpleStubGenerator,
                                     IBarbarianTribeFactory barbarianTribeFactory,
                                     IBarbarianTribeConfigurator barbarianTribeConfigurator,
                                     IRegionManager regionManager,
                                     DefaultMultiObjectLock.Factory multiObjectLockFactory)
        {
            this.dbManager = dbManager;
            this.simpleStubGenerator = simpleStubGenerator;
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
            throw new NotImplementedException();
        }

        public bool TryGetBarbarianTribe(uint id, out IBarbarianTribe barbarianTribe)
        {
            return barbarianTribes.TryGetValue(id, out barbarianTribe);
        }

        public void Generate(int count)
        {
            for (var i = 0; i < count; ++i)
            {
                string name;
                byte level;
                uint x, y;

                if (!barbarianTribeConfigurator.Next(out name, out level, out x, out y))
                {
                    break;
                }

                IBarbarianTribe barbarianTribe = barbarianTribeFactory.CreateBarbarianTribe((uint)idGenerator.GetNext(), name, level, x, y, 10);
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
            }            
        }

        public void Respawn(IBarbarianTribe barbarianTribe)
        {
            throw new NotImplementedException();
        }

        public void RelocateIdle(TimeSpan duration)
        {

        }

        public IEnumerable<Unit> GenerateNeutralStub(IBarbarianTribe barbarianTribe)
        {
            ISimpleStub simpleStub;
            simpleStubGenerator.Generate(barbarianTribe.Lvl,
                                         10,
                                         Config.stronghold_npc_randomness,
                                         (int)barbarianTribe.Id,
                                         out simpleStub);

            return simpleStub.ToUnitList(FormationType.Normal);
        }
    }
}
