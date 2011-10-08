#region

using System;
using System.Linq;
using Game.Data;
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

            return (100 - overtime*10) * count * 300 / 100;
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

        internal static double MoveTimeMod(City city, int distance, bool isAttacking)
        {
            int mod = 0;
            foreach (Effect effect in city.Technologies.GetEffects(EffectCode.TroopSpeedMod, EffectInheritance.All))
            {
                if ((((string)effect.Value[1]).ToUpper() == "ATTACK" && isAttacking) || (((string)effect.Value[1]).ToUpper() == "DEFENSE" && !isAttacking))
                {
                    mod += (int)effect.Value[0];
                }
                else if (((string)effect.Value[1]).ToUpper() == "DISTANCE" && distance > (int)effect.Value[2])
                {
                    mod += (int)effect.Value[0];
                }
            }
            return (double)100 / (Math.Max(1, mod + 100));
        }

        /// <summary>
        /// Get number of seconds it takes to move object by 1 tile
        /// </summary>
        /// <param name="speed">Objects speed</param>
        /// <returns></returns>
        internal static int MoveTime(byte speed) {      
            // 60 is second per square, 12 is the average speed for all troops
            // second per square is lowered from 80 to 60. 3/9/2011
            return 60 * (100 - ((speed - 12) * 5)) / 100;
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

            return (int)((43200 / (-6.845 * Math.Log(laborTotal) + 55)) * rateBonus);
        }
    }
}