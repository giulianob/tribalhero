#region

using System;
using Game.Data;
using Game.Setup;
using System.Linq;

#endregion

namespace Game.Logic {
    public partial class Formula {
        internal static double SendTime(int distance) {
            return MoveTime(11) * distance;
        }

        internal static double TradeTime(Structure structure, int quantity)
        {
            return quantity * 15;
        }

        internal static int LaborMoveTime(Structure structure, byte count, TechnologyManager technologyManager) {
          //  int[] discount = { 0, 2, 4, 6, 8, 10, 15, 20, 30, 50, 80 };
            int[] discount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach( Structure obj in structure.City ) {
                if( ObjectTypeFactory.IsStructureType("University",obj) ) {
                    return (int)((100 - discount[obj.Lvl]) * count * 300 * Config.seconds_per_unit / 100);
                }
            }
            return (int)(count * 300 * Config.seconds_per_unit);
        }

        private static int TimeDiscount(int lvl) {
            int[] discount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 15, 20, 30, 40 };
            return discount[lvl];
        }

        public static int TrainTime(int baseValue, int structureLvl, TechnologyManager em) {
            return (int)(baseValue * Config.seconds_per_unit * (100 - TimeDiscount(structureLvl)) / 100);
        }

        public static int BuildTime(int baseValue, City city, TechnologyManager em) {
            Structure univeristy = city.FirstOrDefault(structure => ObjectTypeFactory.IsStructureType("University", structure));
            int buildtime = (int)(baseValue * (100 - (univeristy==null?0:univeristy.Stats.Labor)*0.25) / 100);
            return (int) (buildtime*Config.seconds_per_unit);
        }

        internal static int MoveTime(byte speed) {
            if (Config.battle_instant_move) return 0;
            return 80 * (100 - ((speed - 11) * 5)) / 100;
        }
    }
}