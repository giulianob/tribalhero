using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util.Locking;

namespace Game.Battle.RewardStrategies
{
    public class CityRewardStrategy : IRewardStrategy
    {
        private readonly IBattleFormulas battleFormulas;

        private readonly ICity city;

        private readonly Formula formula;

        private readonly ILocker locker;

        public CityRewardStrategy(ICity city, IBattleFormulas battleFormulas, Formula formula, ILocker locker)
        {
            this.city = city;
            this.battleFormulas = battleFormulas;
            this.formula = formula;
            this.locker = locker;
        }

        public void RemoveLoot(IBattleManager battleManager, int attackIndex, ICombatObject attacker, ICombatObject defender, out Resource actualLoot)
        {
            if (attackIndex != 0 || battleManager.Round < Config.battle_loot_begin_round)
            {
                actualLoot = new Resource();
                return;
            }

            var loot = battleFormulas.GetRewardResource(attacker, defender);
            city.BeginUpdate();
            city.Resource.Subtract(loot, formula.HiddenResource(city, true), out actualLoot);
            city.EndUpdate();
        }

        public void GiveAttackerRewards(ICombatObject attacker, int attackPoints, Resource loot)
        {
            attacker.ReceiveReward(attackPoints, loot);
        }

        public void GiveDefendersRewards(ICombatObject attacker, int defensePoints, Resource loot)
        {
            // Any loot being added to the defender is loot dropped by the attacker
            // so give back to original city
            if (!loot.Empty)
            {
                city.BeginUpdate();
                city.Resource.Add(loot);
                city.EndUpdate();
            }

            if (defensePoints > 0)
            {
                attacker.ReceiveReward(defensePoints, null);
            }
        }
    }
}