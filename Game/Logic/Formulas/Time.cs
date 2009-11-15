using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Logic {
    public partial class Formula {
        internal static int LaborMoveTime(Game.Data.Structure structure, byte count, Game.Data.TechnologyManager technologyManager) {
            return (int)(60 * count * Setup.Config.seconds_per_unit);
        }

        public static int TrainTime(int base_value, int structure_bonus_percent, TechnologyManager em) {
            //            int buildtime = base_value * em.Min(EffectCode.TrainTimeMultiplier, EffectInheritance.SelfAll, 0) / 100;
            return (int)(base_value * Setup.Config.seconds_per_unit * (100-structure_bonus_percent) /100);
        }
        public static int BuildTime(int base_value, TechnologyManager em) {
            int buildtime = base_value - em.Sum(EffectCode.BuildTimeMultiplier, EffectInheritance.Self);
            return (int)(buildtime * Setup.Config.seconds_per_unit);
        }

        internal static int MoveTime(byte speed) {
            return 20 - speed;
        }

    }
}
