using System.Collections.Generic;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.BarbarianTribe;

namespace Game.Battle.RewardStrategies
{
    public class BarbarianTribeRewardStrategy : IRewardStrategy
    {
        private readonly IBattleFormulas battleFormulas;

        private readonly IBarbarianTribe barbarianTribe;

        public BarbarianTribeRewardStrategy(IBarbarianTribe barbarianTribe, IBattleFormulas battleFormulas)
        {
            this.barbarianTribe = barbarianTribe;
            this.battleFormulas = battleFormulas;
        }

        public void RemoveLoot(IBattleManager battleManager, int attackIndex, ICombatObject attacker, ICombatObject defender, out Resource actualLoot)
        {            
            if (attackIndex != 0)
            {
                actualLoot = new Resource();
                return;
            }

            var loot = battleFormulas.GetRewardResource(attacker, defender);
            barbarianTribe.BeginUpdate();
            barbarianTribe.Resource.Subtract(loot, out actualLoot);
            barbarianTribe.EndUpdate();
        }


        public void GiveAttackerRewards(ICombatObject attacker, int attackPoints, Resource loot)
        {
            // Barb tribes dont give any attack points so just give them loot
            attacker.ReceiveReward(0, loot);
        }

        public void GiveDefendersRewards(ICombatObject attacker, int defensePoints, Resource loot)
        {
            if (!loot.Empty)    
            {
                barbarianTribe.BeginUpdate();                
                barbarianTribe.Resource.Add(loot);
                barbarianTribe.EndUpdate();
            }
        }
    }
}