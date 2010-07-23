#region

using System;
using Game.Data;
using Game.Setup;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Game.Logic
{
    public partial class Formula
    {

        /// <summary>
        /// Returns the cost for building the specified structure
        /// </summary>
        /// <param name="city"></param>
        /// <param name="type"></param>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public static Resource StructureCost(City city, uint type, byte lvl)
        {
            if (city.Battle == null)
                return StructureFactory.GetCost((int)type, lvl);

            return StructureFactory.GetCost((int)type, lvl) * 1.5;
        }

        /// <summary>
        /// Returns the cost for training the specified unit
        /// </summary>
        /// <param name="city"></param>
        /// <param name="type"></param>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public static Resource UnitCost(City city, ushort type, byte lvl)
        {
            if (city.Battle == null)
                return UnitFactory.GetCost(type, lvl);

            return UnitFactory.GetCost(type, lvl) * 1.5;
        }

        /// <summary>
        /// Returns the amount of HP that can be repaired by the specified structure
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public static ushort RepairRate(Structure structure)
        {
            return (ushort)(structure.Stats.Labor * 10);
        }

        /// <summary>
        /// Returns the cost for repairing the specified amount
        /// </summary>
        /// <param name="city"></param>
        /// <param name="repairPower"></param>
        /// <returns></returns>
        public static Resource RepairCost(City city, ushort repairPower)
        {
            int lumber = repairPower;
            foreach (Effect effect in city.Technologies.GetEffects(EffectCode.RepairSaving, EffectInheritance.ALL))
                lumber -= repairPower * (int)effect.value[0] / 100;
            return new Resource(0, 0, 0, lumber, 0);
        }

        public static int GetLaborRate(int laborTotal)
        {
            if (laborTotal < 140) laborTotal = 140;
            return (int)(86400 / (-6.845 * Math.Log(laborTotal) + 55));
        }

        public static ushort LaborMoveMax(Structure structure)
        {
            return (ushort)Math.Max(5, Math.Ceiling(structure.Stats.Base.MaxLabor / 10.0f));
        }

        public static double MarketTax(Structure structure)
        {
            switch (structure.Lvl)
            {
                case 1:
                    return .25;
                case 2:
                    return .20;
                case 3:
                    return .15;
                case 4:
                    return .10;
                case 5:
                    return .05;
            }
            throw new Exception("WTF");
        }

        static int[] rateCrop = { 0, 100, 200, 300, 500, 1000 };
        static int[] rateWood = { 0, 100, 200, 300, 500, 1000 };
        static int[] rateGold = { 0, 0, 50, 100, 200, 400 };
        static int[] rateIron = { 0, 0, 0, 50, 100, 200 };
        public static Resource HiddenResource(City city)
        {
            Resource resource = new Resource();
            foreach (Structure structure in city.Where(x => ObjectTypeFactory.IsStructureType("Basement", x)))
            {
                resource.add(rateCrop[structure.Lvl], rateGold[structure.Lvl], rateIron[structure.Lvl], rateWood[structure.Lvl], 0);
            }
            return resource;
        }

    }
}