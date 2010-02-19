#region

using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {
        internal static int LaborMoveTime(Structure structure, byte count, TechnologyManager technologyManager) {
            return (int) (300 * (count+ (count/10*count/10))*Config.seconds_per_unit);
        }

        private static int TimeDiscount(int lvl) {
            int[] discount = { 0, 0, 0, 0, 0, 5, 10, 15, 20, 30, 40 };
            return discount[lvl];
        }

        public static int TrainTime(int baseValue, int structureLvl, TechnologyManager em) {
            //            int buildtime = baseValue * em.Min(EffectCode.TrainTimeMultiplier, EffectInheritance.SELF_ALL, 0) / 100;
            return (int)(baseValue * Config.seconds_per_unit * (100 - TimeDiscount(structureLvl)) / 100);
        }

        public static int BuildTime(int baseValue, int mainBuildingLvl, TechnologyManager em) {
            int buildtime = (baseValue * (100 - TimeDiscount(mainBuildingLvl)) / 100) - em.Sum(EffectCode.BuildTimeMultiplier, EffectInheritance.SELF);
            return (int) (buildtime*Config.seconds_per_unit);
        }

        internal static int MoveTime(byte speed) {
            return 20 - speed;
        }
    }
}