#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        internal static int SendTime(Structure structure, int distance)
        {
            return MoveTime(11) * distance * 100 / (100 + structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self).DefaultIfEmpty().Max(x => x==null?0:(int)x.Value[0]));
        }

        internal static int TradeTime(Structure structure, int quantity)
        {
            return quantity * 15 * 100 / (100 + structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self).DefaultIfEmpty().Max(x => x == null ? 0 : (int)x.Value[0]));
        }

        internal static int LaborMoveTime(Structure structure, byte count, TechnologyManager technologyManager)
        {
            var effects = structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod, EffectInheritance.All);
            int overtime = 0;
            if (effects.Count > 0)
                overtime = effects.Max(x => (int)x.Value[0]);

            return (100 - overtime*10) * count * 180 / 100;
        }

        private static int TimeDiscount(int lvl)
        {
            int[] discount = {0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 15, 20, 30, 40};
            return discount[lvl];
        }

        public static int TrainTime(int baseValue, int structureLvl, TechnologyManager em)
        {
            return baseValue*(100 - TimeDiscount(structureLvl))/100;
        }

        public static int BuildTime(int baseValue, City city, TechnologyManager em)
        {
            Structure university = city.FirstOrDefault(structure => Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("University", structure));
            return (int)(baseValue*(100 - (university == null ? 0 : university.Stats.Labor)*0.25)/100);
        }

        /// <summary>
        /// Get number of seconds it takes to move object by 1 tile
        /// </summary>
        /// <param name="speed">Objects speed</param>
        /// <returns></returns>
        public static int MoveTime(byte speed) {      
            // 60 is second per square, 12 is the average speed for all troops
            // second per square is lowered from 80 to 60. 3/9/2011
            return 60 * (100 - ((speed - 12) * 5)) / 100;
        }

        private static int DoubleTimeTotal(int moveTime, int distance, double bonusPercentage, int forEveryDistance)
        {
            int total = 0;
            int index = 0;
            int tiles = 0;
            while (distance > 0)
            {
                tiles = distance > forEveryDistance ? forEveryDistance : distance;
                total += (int)(moveTime * tiles / (1 + bonusPercentage * index++));
                distance -= tiles;
            }
            return total;
        }

        public static int MoveTimeTotal(byte speed, int distance, bool isAttacking, List<Effect> effects)
        {
            var moveTime = MoveTime(speed);
            double bonus = 0;
            double rushMod = 0;

            foreach (var effect in effects.Where(x => x.Id == EffectCode.TroopSpeedMod))
            {
                // getting rush attack/defense bonus;
                if ((((string)effect.Value[1]).ToUpper() == "ATTACK" && isAttacking) || (((string)effect.Value[1]).ToUpper() == "DEFENSE" && !isAttacking))
                {
                    rushMod += (int)effect.Value[0];
                }
                        // getting double time bonus
                else if (((string)effect.Value[1]).ToUpper() == "DISTANCE")
                {
                    bonus = (int)effect.Value[0] > bonus ? (int)effect.Value[0] : bonus;
                }
            }

            var bonusPercentage = bonus/100;
            rushMod = 100/(Math.Max(1, rushMod + 100));
            return (int)(DoubleTimeTotal(moveTime, distance, bonusPercentage, 500)*rushMod*Config.seconds_per_unit);
        }

        /// <summary>
        /// Returns number of seconds it takes to get 1 labor
        /// </summary>
        /// <param name = "laborTotal"></param>
        /// <param name = "city"></param>
        /// <returns></returns>
        public static int GetLaborRate(int laborTotal, City city)
        {
            if (laborTotal < 140)
                laborTotal = 140;

            var effects = city.Technologies.GetEffects(EffectCode.LaborTrainTimeMod, EffectInheritance.SelfAll);
            double rateBonus = 1;
            if (effects.Count > 0)
            {
                rateBonus = (effects.Min(x => (int)x.Value[0])*10)/100f;
                if (effects.Count > 1)
                    rateBonus *= Math.Pow(0.92, effects.Count - 1); // for every extra tribal gathering, you gain 8 % each
            }
            double newMultiplier = Math.Min(2, 3- (double)laborTotal/400);
            newMultiplier = Math.Max(1,newMultiplier);
            return (int)((43200 / ((-6.845 * Math.Log(laborTotal / 1.3 - 100) + 55) * newMultiplier)) * rateBonus);
        }

        public static TimeSpan ReadCsvTimeFormat(string time)
        {
            if (time.StartsWith("!"))
            {
                return TimeSpan.FromSeconds(int.Parse(time.Remove(0, 1))/Config.seconds_per_unit);
            }

            return TimeSpan.FromSeconds(int.Parse(time));
        }

        public static double GetBattleInterval(int count)
        {
            // at 400 objects, the reduction is cap'ed at 20% of the original speed.
            return Config.battle_turn_interval*100/(100 + Math.Min(500, count));
        }
    }
}