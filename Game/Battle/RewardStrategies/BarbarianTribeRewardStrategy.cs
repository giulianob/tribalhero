using System.Collections.Generic;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.BarbarianTribe;

namespace Game.Battle.RewardStrategies
{
    public class BarbarianTribeRewardStrategy : IRewardStrategy
    {
        private readonly BattleFormulas battleFormulas;

        private readonly IBarbarianTribe barbarianTribe;

        public BarbarianTribeRewardStrategy(IBarbarianTribe barbarianTribe, BattleFormulas battleFormulas)
        {
            this.barbarianTribe = barbarianTribe;
            this.battleFormulas = battleFormulas;
        }

        public void RemoveLoot(ICombatObject attacker, ICombatObject defender, out Resource actualLoot)
        {
            actualLoot = new Resource();
            var loot = battleFormulas.GetRewardResource(attacker, defender);
            barbarianTribe.BeginUpdate();
            barbarianTribe.Resource.Subtract(loot, out actualLoot);
            barbarianTribe.EndUpdate();
        }

        public void GiveAttackerRewards(ICombatObject attacker, int attackPoints, Resource loot)
        {
            attacker.ReceiveReward(attackPoints, loot);
        }

        public void GiveDefendersRewards(IEnumerable<ICombatObject> defenders, int attackPoints, Resource loot)
        {
            // Any loot being added to the defender is loot dropped by the attacker
            if (!loot.Empty)
            {
                barbarianTribe.BeginUpdate();                
                barbarianTribe.Resource.Add(loot);
                barbarianTribe.EndUpdate();
            }
        }
    }
}