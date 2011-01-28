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
        internal static int SendTime(Structure structure, int distance)
        {
            return MoveTime(11) * distance * 100 / (100 + structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self).DefaultIfEmpty().Max(x => x==null?0:(int)x.Value[0]));
        }

        internal static int TradeTime(Structure structure, int quantity)
        {
            int mod = structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self).DefaultIfEmpty().Max(x => x == null ? 0 : (int)x.Value[0]);
            return quantity * 15 * 100 / (100 + structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self).DefaultIfEmpty().Max(x => x == null ? 0 : (int)x.Value[0]));
        }

        internal static int LaborMoveTime(Structure structure, byte count, TechnologyManager technologyManager) {
            int overtime = structure.City.Technologies.GetEffects(EffectCode.HaveTechnology, EffectInheritance.All).DefaultIfEmpty(new Effect { Value = new object[] { 20001, 0 } }).Max(x => (int)x.Value[0] == 20001 ? (int)x.Value[1] : 0);
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
            Structure univeristy = city.FirstOrDefault(structure => ObjectTypeFactory.IsStructureType("University", structure));
            var buildtime = (int)(baseValue*(100 - (univeristy == null ? 0 : univeristy.Stats.Labor)*0.25)/100);
            return (int)(buildtime*Config.seconds_per_unit);
        }


        internal static double MoveTimeMod(City city, int distance, bool isAttacking) {
            int mod=0;
            foreach (Effect effect in city.Technologies.GetEffects(EffectCode.TroopSpeedMod, EffectInheritance.All)) {
                if ((((string)effect.Value[1]).StartsWith("ATTACK", StringComparison.CurrentCultureIgnoreCase) && isAttacking) ||
                    (((string)effect.Value[1]).StartsWith("DEFENSE", StringComparison.CurrentCultureIgnoreCase) && !isAttacking)) {
                    mod += (int)effect.Value[0];
                } else if (((string)effect.Value[1]).StartsWith("DISTANCE", StringComparison.CurrentCultureIgnoreCase)) {
                    if (distance > (int)effect.Value[2]) {
                        mod += (int)effect.Value[0];
                    }
                }
            }
            return (double)100 / (mod + 100);
        }

        internal static int MoveTime(byte speed) {
            if (Config.battle_instant_move) return 0;
            return 80 * (100 - ((speed - 11) * 5)) / 100;
        }
    }
}