using Game.Map;
using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Battle
{
    public class CombatListFactory : ICombatListFactory
    {
        private readonly IKernel kernel;

        public CombatListFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ICombatList GetCombatList()
        {
            return new CombatList(kernel.Get<IDbManager>(), kernel.Get<ITileLocator>(), kernel.Get<IBattleFormulas>());
        }
    }
}