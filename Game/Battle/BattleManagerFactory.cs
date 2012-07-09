using Game.Comm.Channel;
using Game.Data;
using Game.Setup;
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

        public IBattleManager CreateBattleManager(uint battleId, BattleLocation location, BattleOwner owner, ICity city)
        {
            var bm = new BattleManager(battleId,
                                       location,
                                       owner,
                                       kernel.Get<IRewardStrategyFactory>().CreateCityRewardStrategy(city),
                                       kernel.Get<IDbManager>(),
                                       kernel.Get<IBattleReport>(),
                                       kernel.Get<ICombatListFactory>(),
                                       kernel.Get<ICombatUnitFactory>(),
                                       kernel.Get<ObjectTypeFactory>());

            new BattleChannel(bm);

            bm.BattleReport.Battle = bm;
            return bm;            
        }

        public IBattleManager CreateBattleManager(BattleLocation location, BattleOwner owner, ICity city)
        {
            var battleId = (uint)BattleReport.BattleIdGenerator.GetNext();
            return CreateBattleManager(battleId, location, owner, city);
        }
    }
}
