using System.Collections.Generic;
using Game.Battle.CombatObjects;
using Game.Data;

namespace Game.Battle.RewardStrategies
{
    public interface IRewardStrategy
    {
        void RemoveLoot(CombatObject attacker, CombatObject defender, out Resource actualLoot);

        void ReturnLoot(Resource loot);

        void GiveAttackerRewards(CombatObject attacker, int attackPoints, Resource loot);

        void GiveDefendersRewards(IEnumerable<CombatObject> defenders, int attackPoints, Resource loot);
    }
}