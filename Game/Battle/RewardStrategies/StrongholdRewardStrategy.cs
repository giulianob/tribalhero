using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stronghold;
using Game.Map;

namespace Game.Battle.RewardStrategies
{
    public class StrongholdRewardStrategy : IRewardStrategy
    {
        private readonly IGameObjectLocator gameObjectLocator;

        private readonly IStronghold stronghold;

        public StrongholdRewardStrategy(IStronghold stronghold, IGameObjectLocator gameObjectLocator)
        {
            this.stronghold = stronghold;
            this.gameObjectLocator = gameObjectLocator;
        }

        public void RemoveLoot(ICombatObject attacker, ICombatObject defender, out Resource actualLoot)
        {
            actualLoot = new Resource();
        }

        public void GiveAttackerRewards(ICombatObject attacker, int attackPoints, Resource loot)
        {
            attacker.ReceiveReward(attackPoints, loot);
        }

        public void GiveDefendersRewards(IEnumerable<ICombatObject> defenders, int attackPoints, Resource loot)
        {
            if (stronghold.Tribe == null)
            {
                return;
            }

            var cityObjectDefenders = defenders.OfType<CityCombatObject>();

            // Give anyone stationed defense points as well
            if (attackPoints > 0)
            {
                foreach (var defendingCity in cityObjectDefenders.Select(co => co.City).Distinct())
                {
                    defendingCity.BeginUpdate();
                    defendingCity.DefensePoint += attackPoints;
                    defendingCity.EndUpdate();
                }

                stronghold.Tribe.DefensePoint += attackPoints;
            }
        }
    }
}