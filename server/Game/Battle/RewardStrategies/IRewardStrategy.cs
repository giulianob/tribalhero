using Game.Battle.CombatObjects;
using Game.Data;

namespace Game.Battle.RewardStrategies
{
    public interface IRewardStrategy
    {
        void RemoveLoot(IBattleManager battleManager, int attackIndex, ICombatObject attacker, ICombatObject defender, out Resource actualLoot);

        void GiveAttackerRewards(ICombatObject attacker, int attackPoints, Resource loot);

        void GiveDefendersRewards(ICombatObject attacker, int defensePoints, Resource loot);
    }
}