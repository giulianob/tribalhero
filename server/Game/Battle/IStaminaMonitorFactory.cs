using Game.Battle.CombatGroups;

namespace Game.Battle
{
    public interface IStaminaMonitorFactory
    {
        StaminaMonitor CreateStaminaMonitor(IBattleManager battleManager, ICombatGroup combatGroup, short initialStamina);
    }
}
