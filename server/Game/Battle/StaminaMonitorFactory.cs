using Game.Battle.CombatGroups;
using Game.Setup;
using Game.Setup.DependencyInjection;

namespace Game.Battle
{
    public class StaminaMonitorFactory : IStaminaMonitorFactory
    {
        private readonly IKernel kernel;

        public StaminaMonitorFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public StaminaMonitor CreateStaminaMonitor(IBattleManager battleManager, ICombatGroup combatGroup, short initialStamina)
        {
            return new StaminaMonitor(battleManager, combatGroup, initialStamina, kernel.Get<IBattleFormulas>(), kernel.Get<IObjectTypeFactory>());
        }
    }
}