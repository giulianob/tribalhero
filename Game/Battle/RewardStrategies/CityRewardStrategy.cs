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

        public void GiveDefendersRewards(IEnumerable<ICombatObject> defenders, int attackPoints, Resource loot)
        {
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

                foreach (var defendingCity in defenders.OfType<CityCombatObject>().Select(co => co.City).Distinct())
                {
                    uniqueCities.Add(defendingCity);
                    defendingCity.BeginUpdate();
                    defendingCity.DefensePoint += attackPoints;
                    defendingCity.EndUpdate();
                }

                var tribes =
                        new List<ITribe>(
                                uniqueCities.Where(w => w.Owner.Tribesman != null)
                                            .Select(s => s.Owner.Tribesman.Tribe)
                                            .Distinct());

                // Need to queue this since the tribe is probably not locked
                // The defense points will be persisted because the DefensePoint
                // call of a tribe persists it immediately. If this changes
                // we may need to do an explicit Save on it.
                ThreadPool.QueueUserWorkItem(delegate
                    {
                        foreach (var tribe in tribes)
                        {
                            using (locker.Lock(tribe))
                            {
                                tribe.DefensePoint += attackPoints;
                            }
                        }
                    });
            }
        }
    }
}