using Game.Data;
using Game.Setup;
using Ninject;
using Ninject.Parameters;

namespace Game.Battle
{
    class BattleManagerFactory
    {
        public BattleManager CreateBattleManager(City owner)
        {
            BattleManager bm = Ioc.Kernel.Get<BattleManager>(new ConstructorArgument("owner", owner));

            // TODO: Fix cyclic dependency
            bm.BattleReport.Battle = bm;

            return bm;
        }
    }
}
