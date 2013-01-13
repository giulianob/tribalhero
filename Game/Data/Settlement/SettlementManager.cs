using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Settlement
{
    public class SettlementManager : ISettlementManager
    {
        private readonly IDbManager dbManager;
        private readonly SimpleStubGenerator simpleStubGenerator;
        private readonly ISettlementFactory settlementFactory;
        private readonly ISettlementConfigurator settlementConfigurator;
        private readonly IRegionManager regionManager;
        private readonly ILocker locker;

        private readonly ConcurrentDictionary<uint, ISettlement> settlements = new ConcurrentDictionary<uint, ISettlement>();
        private readonly LargeIdGenerator idGenerator = new LargeIdGenerator(10000, 5000);

        public SettlementManager(IDbManager dbManager,
                                 SimpleStubGenerator simpleStubGenerator,
                                 ISettlementFactory settlementFactory,
                                 ISettlementConfigurator settlementConfigurator,
                                 IRegionManager regionManager,
                                 ILocker locker)
        {
            this.dbManager = dbManager;
            this.simpleStubGenerator = simpleStubGenerator;
            this.settlementFactory = settlementFactory;
            this.settlementConfigurator = settlementConfigurator;
            this.regionManager = regionManager;
            this.locker = locker;
        }

        #region Implementation of ISettlementManager

        public int Count
        {
            get
            {
                return settlements.Count;
            }
        }

        public void DbLoaderAdd(ISettlement settlement)
        {
            throw new NotImplementedException();
        }

        public bool TryGetSettlement(uint id, out ISettlement settlement)
        {
            return settlements.TryGetValue(id, out settlement);
        }

        public void Generate(int count)
        {
            for (var i = 0; i < count; ++i)
            {
                string name;
                byte level;
                uint x, y;

                if (!settlementConfigurator.Next(out name, out level, out x, out y))
                {
                    break;
                }

                ISettlement settlement = settlementFactory.CreateSettlement((uint)idGenerator.GetNext(),
                                                                            name,
                                                                            level,
                                                                            x,
                                                                            y,
                                                                            10);
                using (locker.Lock(settlement))
                {
                    settlements.AddOrUpdate(settlement.Id, settlement, (id, old) => settlement);
                    settlement.BeginUpdate();
                    settlement.State = GameObjectState.NormalState();
                    regionManager.Add(settlement);
                    settlement.EndUpdate();
                }
            }
        }

        public void Respawn(ISettlement settlement)
        {
            throw new NotImplementedException();
        }

        public void RelocateIdle(TimeSpan duration)
        {

        }

        public IEnumerable<Unit> GenerateNeutralStub(ISettlement settlement)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
