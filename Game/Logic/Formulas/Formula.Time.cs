#region

using System;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Setup;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        public virtual int SendTime(IStructure structure, int distance)
        {
            return (int)(MoveTime(11) * distance * 100 /
                         (100 +
                          structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self)
                                  .DefaultIfEmpty()
                                  .Max(x => x == null ? 0 : (int)x.Value[0])));
        }

        public virtual int TradeTime(IStructure structure, Resource resource)
        {
            return TradeTime(structure, 0);
        }

        public virtual int TradeTime(IStructure structure, int quantity)
        {
            return 14400 * 100 /
                   (100 +
                    structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self)
                             .DefaultIfEmpty()
                             .Max(x => x == null ? 0 : (int)x.Value[0]));
        }

        public virtual int LaborMoveTime(IStructure structure, byte count, ITechnologyManager technologyManager)
        {
            var effects = structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod, EffectInheritance.All);
            int overtime = 0;
            if (effects.Count > 0)
            {
                overtime = effects.Max(x => (int)x.Value[0]);
            }

            return (100 - overtime * 10) * count * 180 / 100;
        }

        private int TimeDiscount(int lvl)
        {
            int[] discount = {0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 15, 20, 30, 40};
            return discount[lvl];
        }

        public virtual int TrainTime(int baseValue, int structureLvl, ITechnologyManager em)
        {
            return baseValue * (100 - TimeDiscount(structureLvl)) / 100;
        }

        public virtual int BuildTime(int baseValue, ICity city, ITechnologyManager em)
        {
            IStructure university =
                    city.FirstOrDefault(structure => ObjectTypeFactory.IsStructureType("University", structure));
            return (int)(baseValue * (100 - (university == null ? 0 : university.Stats.Labor) * 0.25) / 100);
        }

        /// <summary>
        ///     Get number of seconds it takes to move object by 1 tile
        /// </summary>
        /// <param name="speed">Objects speed</param>
        /// <returns></returns>
        public virtual decimal MoveTime(decimal speed)
        {
            // 60 is second per square, 12 is the average speed for all troops
            // second per square is lowered from 80 to 60. 3/9/2011
            // Base speed is 45 tile/hr, Each speed is extra +5 tile/hr 2/10/2013
            return Math.Round(3600 / (45 + speed * 5), 1);
        }

        private int DoubleTimeTotal(decimal moveTime, int distance, decimal bonusPercentage, int doubleTimeDistance)
        {
            decimal total;

            if(distance>doubleTimeDistance)
            {
                total = moveTime * doubleTimeDistance + (distance - doubleTimeDistance) * moveTime / (1 + bonusPercentage);
            } else
            {
                total = moveTime * distance;
            }
            return (int)total;
        }

        public virtual int MoveTimeTotal(ITroopStub stub, int distance, bool isAttacking)
        {
            var moveTime = MoveTime(stub.Speed);
            decimal bonus = 0;
            decimal rushMod = 0;

            foreach (var effect in stub.City.Technologies.GetEffects(EffectCode.TroopSpeedMod))
            {
                // Getting rush attack/defense bonus;
                if ((((string)effect.Value[1]).ToUpper() == "ATTACK" && isAttacking) ||
                    (((string)effect.Value[1]).ToUpper() == "DEFENSE" && !isAttacking))
                {
                    rushMod += (int)effect.Value[0];
                }
                // Getting double time bonus
                else if (((string)effect.Value[1]).ToUpper() == "DISTANCE")
                {
                    bonus += (int)effect.Value[0];
                }
            }

            var bonusPercentage = bonus / 100;
            var doubletimetotal = DoubleTimeTotal(moveTime, distance, bonusPercentage, 500);
            return (int)(doubletimetotal * 100m * (decimal)Config.seconds_per_unit / Math.Max(1m, rushMod + 100m));
        }

        /// <summary>
        ///     Returns number of seconds it takes to get 1 labor
        /// </summary>
        /// <param name="laborTotal"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public virtual int GetLaborRate(int laborTotal, ICity city)
        {
            if (laborTotal < 140)
            {
                laborTotal = 140;
            }

            var effects = city.Technologies.GetEffects(EffectCode.LaborTrainTimeMod, EffectInheritance.SelfAll);
            double rateBonus = 1;
            if (effects.Count > 0)
            {
                rateBonus = (effects.Min(x => (int)x.Value[0]) * 10) / 100f;
                if (effects.Count > 1)
                {
                    rateBonus *= Math.Pow(0.92, effects.Count - 1);
                            // for every extra tribal gathering, you gain 8 % each
                }
            }
            double newMultiplier = Math.Min(2, 3 - (double)laborTotal / 400);
            newMultiplier = Math.Max(1, newMultiplier);
            return (int)((43200 / ((-6.845 * Math.Log(laborTotal / 1.3 - 100) + 55) * newMultiplier)) * rateBonus * Config.seconds_per_unit);
        }

        public virtual TimeSpan ReadCsvTimeFormat(string time)
        {
            if (time.StartsWith("!"))
            {
                return TimeSpan.FromSeconds(int.Parse(time.Remove(0, 1)) / Config.seconds_per_unit);
            }

            return TimeSpan.FromSeconds(int.Parse(time));
        }

        public virtual double GetBattleInterval(int count)
        {
            // at 400 objects, the reduction is cap'ed at 20% of the original speed.
            var ret = Config.battle_turn_interval * 100 / (100 + Math.Min(500, count));
            return Config.server_production ? Math.Max(4, ret) : ret;
        }
    }
}