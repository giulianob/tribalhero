using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic.Formulas;
using Game.Util.Locking;

namespace Game.Battle.RewardStrategies
{
    public class CityRewardStrategy : IRewardStrategy
    {
        private readonly City city;

        private readonly BattleFormulas battleFormulas;

        private readonly Formula formula;

        public CityRewardStrategy(City city, BattleFormulas battleFormulas, Formula formula)
        {
            this.city = city;
            this.battleFormulas = battleFormulas;
            this.formula = formula;
        }

        public void RemoveLoot(CombatObject attacker, CombatObject defender, out Resource actualLoot)
        {
            var loot = battleFormulas.GetRewardResource(attacker, defender);
            city.BeginUpdate();
            city.Resource.Subtract(loot, formula.HiddenResource(city, true), out actualLoot);
            city.EndUpdate();
        }

        public void ReturnLoot(Resource loot)
        {
            city.BeginUpdate();
            city.Resource.Add(loot);
            city.EndUpdate();
        }

        public void GiveAttackerRewards(CombatObject attacker, int attackPoints, Resource loot)
        {
            attacker.ReceiveReward(attackPoints, loot);
        }

        public void GiveDefendersRewards(IEnumerable<CombatObject> defenders, int attackPoints, Resource loot)
        {
            var cityObjectDefenders = defenders.OfType<CityCombatObject>();

            // Any loot being added to the defender is loot dropped by the attacker
            if (!loot.Empty)
            {
                city.BeginUpdate();
                city.Resource.Add(loot);
                city.EndUpdate();
            }

            // Give anyone stationed defense points as well
            // DONT convert this to LINQ because I'm not sure how it might affect the list inside of the loop that keeps changing
            if (attackPoints > 0)
            {
                var uniqueCities = new HashSet<ICity>();

                foreach (var defendingCity in cityObjectDefenders.Select(co => co.City).Distinct())
                {
                    defendingCity.BeginUpdate();
                    defendingCity.DefensePoint += attackPoints;
                    defendingCity.EndUpdate();
                }

                var tribes = new List<ITribe>(uniqueCities.Where(w => w.Owner.Tribesman != null).Select(s => s.Owner.Tribesman.Tribe).Distinct());

                // Need to queue this since the tribe is probably not locked
                // The defense points will be persisted because the DefensePoint
                // call of a tribe persists it immediately. If this changes
                // we may need to do an explicit Save on it.
                ThreadPool.QueueUserWorkItem(delegate
                    {
                        foreach (var tribe in tribes)
                        {
                            using (Concurrency.Current.Lock(tribe))
                            {
                                tribe.DefensePoint += attackPoints;
                            }
                        }
                    });
            }
        }
    }
}
