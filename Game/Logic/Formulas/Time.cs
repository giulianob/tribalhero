#region

using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {
        internal static int LaborMoveTime(Structure structure, byte count, TechnologyManager technologyManager) {
            return (int) (60*count*Config.seconds_per_unit);
        }

        public static int TrainTime(int baseValue, int structureBonusPercent, TechnologyManager em) {
            //            int buildtime = baseValue * em.Min(EffectCode.TrainTimeMultiplier, EffectInheritance.SELF_ALL, 0) / 100;
            return (int) (baseValue*Config.seconds_per_unit*(100 - structureBonusPercent)/100);
        }

        public static int BuildTime(int baseValue, TechnologyManager em) {
            int buildtime = baseValue - em.Sum(EffectCode.BuildTimeMultiplier, EffectInheritance.SELF);
            return (int) (buildtime*Config.seconds_per_unit);
        }

        internal static int MoveTime(byte speed) {
            return 20 - speed;
        }
    }
}