using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Comm.Channel;
using Game.Data;
using Game.Data.Stronghold;
using Ninject;
using Persistance;

namespace Game.Battle
{
    class BattleManagerFactory : IBattleManagerFactory
    {
        private readonly IKernel kernel;

        public BattleManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IBattleManager CreateBattleManager(uint battleId, BattleLocation battleLocation, BattleOwner battleOwner, ICity city)
        {
            var bm = new BattleManager(battleId,
                                       battleLocation,
                                       battleOwner,
                                       kernel.Get<IRewardStrategyFactory>().CreateCityRewardStrategy(city),
                                       kernel.Get<IDbManager>(),
                                       kernel.Get<IBattleReport>(),
                                       kernel.Get<ICombatListFactory>(),
                                       kernel.Get<BattleFormulas>());

// ReSharper disable ObjectCreationAsStatement
            new BattleChannel(bm);
// ReSharper restore ObjectCreationAsStatement

            bm.BattleReport.Battle = bm;
            return bm;
        }

        public IBattleManager CreateBattleManager(BattleLocation location, BattleOwner owner, ICity city)
        {
            var battleId = (uint)BattleReport.BattleIdGenerator.GetNext();
            return CreateBattleManager(battleId, location, owner, city);
        }

        public IBattleManager CreateBattleManager(uint battleId, BattleLocation battleLocation, BattleOwner battleOwner, IStronghold stronghold)
        {
            var bm = new BattleManager(battleId,
                                       battleLocation,
                                       battleOwner,
                                       kernel.Get<IRewardStrategyFactory>().CreateStrongholdRewardStrategy(stronghold),
                                       kernel.Get<IDbManager>(),
                                       kernel.Get<IBattleReport>(),
                                       kernel.Get<ICombatListFactory>(),
                                       kernel.Get<BattleFormulas>());

// ReSharper disable ObjectCreationAsStatement
            new BattleChannel(bm);
// ReSharper restore ObjectCreationAsStatement

            bm.BattleReport.Battle = bm;
            return bm;
        }

        public IBattleManager CreateBattleManager(BattleLocation battleLocation, BattleOwner battleOwner, IStronghold stronghold)
        {
            var battleId = (uint)BattleReport.BattleIdGenerator.GetNext();
            return CreateBattleManager(battleId, battleLocation, battleOwner, stronghold);
        }
    }
}
