#region

using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {
        internal static Resource StructureCost(City city, uint type, byte lvl) {
            if (city.Battle == null)
                return StructureFactory.GetCost((int) type, lvl);
            else
                return StructureFactory.GetCost((int) type, lvl)*1.5;
        }

        internal static Resource UnitCost(City city, ushort type, byte lvl) {
            if (city.Battle == null)
                return UnitFactory.GetCost(type, lvl);
            else
                return UnitFactory.GetCost(type, lvl)*1.5;
        }

        internal static ushort RepairRate(Structure structure) {
            //repairPower += (ushort)(structure.Stats.Base.Lvl * (50 + city.MainBuilding.Stats.Base.Lvl * 10));
            return (ushort) (structure.Stats.Labor*10);
        }

        internal static Resource RepairCost(City city, ushort repairPower) {
            int lumber = repairPower;
            foreach (Effect effect in city.Technologies.GetEffects(EffectCode.RepairSaving, EffectInheritance.All))
                lumber -= repairPower*(int) effect.value[0]/100;
            return new Resource(0, 0, 0, lumber, 0);
        }
    }
}