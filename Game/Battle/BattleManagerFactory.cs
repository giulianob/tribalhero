using System;
using Game.Data;
using Ninject;
using Ninject.Parameters;

namespace Game.Battle
{
    class BattleManagerFactory : IBattleManagerFactory
    {
        private readonly IKernel kernel;

        public BattleManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IBattleManager CreateBattleManager(ICity owner)
        {
            var bm = kernel.Get<BattleManager>(new ConstructorArgument("owner", owner));
            bm.BattleReport.Battle = bm;
            return bm;
        }
    }
}
