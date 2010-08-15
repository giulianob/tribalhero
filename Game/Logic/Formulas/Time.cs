#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {
        internal static double SendTime(int distance) {
            return distance * 2000;
        }

        internal static double TradeTime(Structure structure)
        {
            return 50 + 50 / structure.Lvl;
        }

        internal static int LaborMoveTime(Structure structure, byte count, TechnologyManager technologyManager) {
            int[] discount = { 0, 2, 4, 6, 8, 10, 15, 20, 30, 50, 80 };
            foreach( Structure obj in structure.City ) {
                if( ObjectTypeFactory.IsStructureType("University",obj) ) {
                    return (int)((100 - discount[obj.Lvl]) * count * 300 * Config.seconds_per_unit / 100);
                }
            }
            return (int)(count * 300 * Config.seconds_per_unit);
        }

        private static int TimeDiscount(int lvl) {
            int[] discount = { 0, 0, 0, 0, 0, 5, 10, 15, 20, 30, 40 };
            return discount[lvl];
        }

        public static int TrainTime(int baseValue, int structureLvl, TechnologyManager em) {
            return (int)(baseValue * Config.seconds_per_unit * (100 - TimeDiscount(structureLvl)) / 100);
        }

        public static int BuildTime(int baseValue, int mainBuildingLvl, TechnologyManager em) {
            int buildtime = (baseValue * (100 - TimeDiscount(mainBuildingLvl)) / 100) - em.Sum(EffectCode.BuildTimeMultiplier, EffectInheritance.SELF);
            return (int) (buildtime*Config.seconds_per_unit);
        }

        internal static int MoveTime(byte speed) {
            return Math.Max(1, 50 - speed);
        }
    }
}