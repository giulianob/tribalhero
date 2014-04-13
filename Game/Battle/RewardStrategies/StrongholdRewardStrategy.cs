using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stronghold;

namespace Game.Battle.RewardStrategies
{
    public class StrongholdRewardStrategy : IRewardStrategy
    {
        private readonly IStronghold stronghold;

        public StrongholdRewardStrategy(IStronghold stronghold)
        {
            this.stronghold = stronghold;
        }

        public void RemoveLoot(IBattleManager battleManager, int attackIndex, ICombatObject attacker, ICombatObject defender, out Resource actualLoot)
        {
            actualLoot = new Resource();
        }

        public void GiveAttackerRewards(ICombatObject attacker, int attackPoints, Resource loot)
        {
            attacker.ReceiveReward(attackPoints, loot);
        }

        public void GiveDefendersRewards(ICombatObject attacker, int defensePoints, Resource loot)
        {
            if (stronghold.Tribe != null && defensePoints > 0)
            {
                stronghold.Tribe.DefensePoint += defensePoints;
            }
        }
    }
}