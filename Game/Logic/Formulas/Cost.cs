using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Logic {
    public partial class Formula {
        internal static Resource StructureCost(City city, uint type, byte lvl) {
            if (city.Battle == null) {
                return StructureFactory.getCost((int)type,(int)lvl);
            } else {
                return StructureFactory.getCost((int)type, (int)lvl) * 1.5;
            }
        }
        internal static Resource UnitCost(City city, ushort type, byte lvl) {
            if (city.Battle == null) {
                return UnitFactory.getCost((int)type, (int)lvl);
            } else {
                return UnitFactory.getCost((int)type, (int)lvl) * 1.5;
            }
        }
    }
}
