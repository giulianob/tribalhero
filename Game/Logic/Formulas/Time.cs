#region

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
            int[] discount = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            foreach (var obj in structure.City)
            {
                if (ObjectTypeFactory.IsStructureType("University", obj))
                    return (int)((100 - discount[obj.Lvl])*count*300*Config.seconds_per_unit/100);
            }
            return (int)(count*300*Config.seconds_per_unit);
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

        internal static int MoveTime(byte speed)
        {
            if (Config.battle_instant_move)
                return 0;
            return 80*(100 - ((speed - 11)*5))/100;
        }
    }
}