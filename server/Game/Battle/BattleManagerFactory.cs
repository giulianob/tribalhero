using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Comm.Channel;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Map;
using Game.Setup.DependencyInjection;
using Game.Util;
using Persistance;

namespace Game.Battle
{
    public class BattleManagerFactory : IBattleManagerFactory
    {
        private readonly IKernel kernel;

        public BattleManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IBattleManager CreateBattleManager(uint battleId,
                                                  BattleLocation battleLocation,
                                                  BattleOwner battleOwner,
                                                  ICity city)
        {

            var battleRandom = new BattleRandom(battleId);
            
            var bm = new BattleManager(battleId,
                                       battleLocation,
                                       battleOwner,
                                       kernel.Get<IRewardStrategyFactory>().CreateCityRewardStrategy(city),
                                       kernel.Get<IDbManager>(),
                                       kernel.Get<IBattleReport>(),
                                       kernel.Get<ICombatListFactory>(),
                                       kernel.Get<IBattleFormulas>(),
                                       new BattleOrder(battleRandom),
                                       battleRandom);

            // ReSharper disable once ObjectCreationAsStatement
            new BattleChannel(bm, kernel.Get<IChannel>());

            bm.BattleReport.Battle = bm;
            return bm;
        }

        public IBattleManager CreateBattleManager(BattleLocation location, BattleOwner owner, ICity city)
        {
            var battleId = BattleReport.BattleIdGenerator.GetNext();
            return CreateBattleManager(battleId, location, owner, city);
        }

        public IBattleManager CreateStrongholdMainBattleManager(uint battleId,
                                                                BattleLocation battleLocation,
                                                                BattleOwner battleOwner,
                                                                IStronghold stronghold)
        {
            var battleRandom = new BattleRandom(battleId);

            var bm = new StrongholdMainBattleManager(battleId,
                                             battleLocation,
                                             battleOwner,
                                             kernel.Get<IRewardStrategyFactory>().CreateStrongholdRewardStrategy(stronghold),
                                             kernel.Get<IDbManager>(),
                                             kernel.Get<IBattleReport>(),
                                             kernel.Get<ICombatListFactory>(),
                                             kernel.Get<IGameObjectLocator>(),                                             
                                             new BattleOrder(battleRandom),   
                                             kernel.Get<IBattleFormulas>(),
                                             battleRandom);

            // ReSharper disable once ObjectCreationAsStatement
            new BattleChannel(bm, kernel.Get<IChannel>());

            bm.BattleReport.Battle = bm;
            return bm;
        }

        public IBattleManager CreateStrongholdMainBattleManager(BattleLocation battleLocation,
                                                                BattleOwner battleOwner,
                                                                IStronghold stronghold)
        {
            var battleId = BattleReport.BattleIdGenerator.GetNext();
            return CreateStrongholdMainBattleManager(battleId, battleLocation, battleOwner, stronghold);
        }

        public IBattleManager CreateStrongholdGateBattleManager(BattleLocation battleLocation,
                                                                BattleOwner battleOwner,
                                                                IStronghold stronghold)
        {
            var battleId = BattleReport.BattleIdGenerator.GetNext();
            return CreateStrongholdGateBattleManager(battleId, battleLocation, battleOwner, stronghold);
        }

        public IBattleManager CreateBarbarianBattleManager(BattleLocation battleLocation, BattleOwner battleOwner, IBarbarianTribe barbarianTribe)
        {
            var battleId = BattleReport.BattleIdGenerator.GetNext();
            return CreateBarbarianBattleManager(battleId, battleLocation, battleOwner, barbarianTribe);
        }

        public IBattleManager CreateBarbarianBattleManager(uint battleId,
                                                           BattleLocation battleLocation,
                                                           BattleOwner battleOwner,
                                                           IBarbarianTribe barbarianTribe)
        {
            var battleRandom = new BattleRandom(battleId);

            var bm = new PublicBattleManager(battleId,
                                             battleLocation,
                                             battleOwner,
                                             kernel.Get<IRewardStrategyFactory>().CreateBarbarianTribeRewardStrategy(barbarianTribe),
                                             kernel.Get<IDbManager>(),
                                             kernel.Get<IBattleReport>(),
                                             kernel.Get<ICombatListFactory>(),
                                             kernel.Get<IBattleFormulas>(),
                                             new BattleOrder(battleRandom),
                                             battleRandom);

            // ReSharper disable once ObjectCreationAsStatement
            new BattleChannel(bm, kernel.Get<IChannel>());

            bm.BattleReport.Battle = bm;
            return bm;
        }

        public IBattleManager CreateStrongholdGateBattleManager(uint battleId,
                                                                BattleLocation battleLocation,
                                                                BattleOwner battleOwner,
                                                                IStronghold stronghold)
        {
            var battleRandom = new BattleRandom(battleId);

            var bm = new BattleManagerGate(battleId,
                                           stronghold,
                                           battleLocation,
                                           battleOwner,
                                           kernel.Get<IRewardStrategyFactory>().CreateStrongholdRewardStrategy(stronghold),
                                           kernel.Get<IDbManager>(),
                                           new BattleReport(new NullBattleReportWriter()),
                                           kernel.Get<ICombatListFactory>(),

                                           kernel.Get<IBattleFormulas>(),
                                           new BattleOrder(battleRandom),
                                           battleRandom);

            // ReSharper disable once ObjectCreationAsStatement
            new BattleChannel(bm, kernel.Get<IChannel>());

            bm.BattleReport.Battle = bm;
            return bm;
        }
    }
}