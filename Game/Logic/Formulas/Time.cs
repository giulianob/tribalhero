#region

using System;
using System.Linq;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        internal static int SendTime(int distance)
        {
            return MoveTime(11)*distance;
        }

        internal static int TradeTime(Structure structure, int quantity)
        {
            return quantity*15;
        }

        internal static int LaborMoveTime(Structure structure, byte count, TechnologyManager technologyManager)
        {
            var effects = structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod, EffectInheritance.All);
            int overtime = 0;
            if (effects.Count > 0)
                overtime = effects.Max(x => (int)x.Value[0]);

            return (int)((100 - overtime*10) * count * 300 * Config.seconds_per_unit / 100);
        }

        private static int TimeDiscount(int lvl)
        {
            int[] discount = {0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 15, 20, 30, 40};
            return discount[lvl];
        }

        public static int TrainTime(int baseValue, int structureLvl, TechnologyManager em)
        {
            return (int)(baseValue*Config.seconds_per_unit*(100 - TimeDiscount(structureLvl))/100);
        }

        public static int BuildTime(int baseValue, City city, TechnologyManager em)
        {
            Structure university = city.FirstOrDefault(structure => ObjectTypeFactory.IsStructureType("University", structure));
            var buildtime = (int)(baseValue*(100 - (university == null ? 0 : university.Stats.Labor)*0.25)/100);
            return (int)(buildtime*Config.seconds_per_unit);
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
            return (double)100/(mod + 100);
        }

        /// <summary>
        /// Get number of seconds it takes to move object by 1 tile
        /// </summary>
        /// <param name="speed">Objects speed</param>
        /// <returns></returns>
        internal static int MoveTime(byte speed) {
            if (Config.battle_instant_move) return 0;
            return 80 * (100 - ((speed - 11) * 5)) / 100;
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
            float rateBonus = 1;
            if (effects.Count > 0)
                rateBonus = (effects.Min(x => (int)x.Value[0]) * 10) / 100f;

            return (int)((43200 / (-6.845 * Math.Log(laborTotal) + 55)) * rateBonus);
        }
    }
}