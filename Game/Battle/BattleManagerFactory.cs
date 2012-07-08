using System;
using Game.Comm.Channel;
using Game.Data;
using Game.Setup;
using Ninject;
using Ninject.Parameters;
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
            var channel = new BattleChannel(city);
            var bm = new BattleManager(battleId,
                                       city,
                                       kernel.Get<IDbManager>(),
                                       channel,
                                       kernel.Get<IBattleReport>(),
                                       kernel.Get<ICombatListFactory>(),
                                       kernel.Get<ICombatUnitFactory>(),
                                       kernel.Get<ObjectTypeFactory>());

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
