using System.Collections.Generic;
using Game.Battle.CombatObjects;
using Game.Data;

namespace Game.Battle.RewardStrategies
{
    public interface IRewardStrategy
    {
        void RemoveLoot(ICombatObject attacker, ICombatObject defender, out Resource actualLoot);

        void GiveAttackerRewards(ICombatObject attacker, int attackPoints, Resource loot);

        void GiveDefendersRewards(IEnumerable<ICombatObject> defenders, int attackPoints, Resource loot);
    }
}