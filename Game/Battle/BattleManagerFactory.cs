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

        public IBattleManager CreateBattleManager(uint battleId, ICity city)
        {            
            var bm = new BattleManager(battleId,
                                       city,
                                       kernel.Get<IDbManager>(),
                                       kernel.Get<IBattleReport>(),
                                       kernel.Get<ICombatListFactory>(),
                                       kernel.Get<ICombatUnitFactory>(),
                                       kernel.Get<ObjectTypeFactory>());

            var battleChannel = new BattleChannel(bm, city);

            bm.BattleReport.Battle = bm;
            return bm;            
        }

        public IBattleManager CreateBattleManager(ICity city)
        {
            var battleId = (uint)BattleReport.BattleIdGenerator.GetNext();
            return CreateBattleManager(battleId, city);
        }
    }
}
