#region

using System;
using Game.Data;
using Game.Setup;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            foreach (Effect effect in city.Technologies.GetEffects(EffectCode.RepairSaving, EffectInheritance.ALL))
                lumber -= repairPower*(int) effect.value[0]/100;
            return new Resource(0, 0, 0, lumber, 0);
        }

        internal static int GetLaborRate(int laborTotal) {
            if (laborTotal < 140) laborTotal = 140;
            return (int)(86400 / (-6.845 * Math.Log(laborTotal) + 55));
        }

        internal static ushort LaborMoveMax(Structure structure) {
            return (ushort)Math.Max(5, Math.Ceiling(structure.Stats.Base.MaxLabor / 10.0f));
        }

        internal static int GetAwayFromRadius(IEnumerable<Effect> effects, byte radius, ushort type) {
            return radius + effects.Sum(x => (x.id == EffectCode.AwayFromStructureMod && (int)x.value[0] == type) ? (int)x.value[1] : 0);
        }

        static int[] rateCrop = { 0, 100, 200, 300, 500, 1000 };
        static int[] rateWood = { 0, 100, 200, 300, 500, 1000 };
        static int[] rateGold = { 0, 0, 50, 100, 200, 400 };
        static int[] rateIron = { 0, 0, 0, 50, 100, 200 };
        internal static Resource HiddenResource(City city) {
            Resource resource = new Resource();
            foreach (Structure structure in city.Where(x=>ObjectTypeFactory.IsStructureType("Basement",x))) {
                resource.add(rateCrop[structure.Lvl], rateGold[structure.Lvl], rateIron[structure.Lvl], rateWood[structure.Lvl], 0);
            }
            return resource;
        }
    }
}