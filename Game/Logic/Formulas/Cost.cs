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

        internal static ushort RepairRate(Structure structure) {
            //repairPower += (ushort)(structure.Stats.Base.Lvl * (50 + city.MainBuilding.Stats.Base.Lvl * 10));
            return (ushort)(structure.Stats.Labor * 10);
        }

        internal static Resource RepairCost(City city, ushort repairPower) {
            int lumber = repairPower;
            foreach (Effect effect in city.Technologies.GetEffects(EffectCode.RepairSaving,EffectInheritance.All)) {
                lumber -= repairPower * (100 - (int)effect.value[0]) / 100;
            }
            return new Resource(0, 0, 0, lumber, 0);
        }
    }
}
