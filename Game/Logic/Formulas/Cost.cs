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
        public static Resource UnitTrainCost(City city, ushort type, byte lvl)
        {
            if (city.Battle == null)
                return UnitFactory.GetCost(type, lvl);

            return UnitFactory.GetCost(type, lvl) * 1.5;
        }

        /// <summary>
        /// Returns the cost for upgrading the specified unit
        /// </summary>
        /// <param name="city"></param>
        /// <param name="type"></param>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public static Resource UnitUpgradeCost(City city, ushort type, byte lvl)
        {
            if (city.Battle == null)
                return UnitFactory.GetUpgradeCost(type, lvl);

            return UnitFactory.GetUpgradeCost(type, lvl) * 1.5;
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

        /// <summary>
        /// Returns number of seconds it takes to get 1 labor
        /// </summary>
        /// <param name="laborTotal"></param>
        /// <returns></returns>
        public static int GetLaborRate(int laborTotal)
        {
            if (laborTotal < 140) laborTotal = 140;
            return (int)(86400 / (-6.845 * Math.Log(laborTotal) + 55))/2;  // Labor time cut in half after first tested.
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
                    return .10;
                case 2:
                    return .05;
                case 3:
                    return 0;
                case 4:
                    return -.05;
                case 5:
                    return -.10;
            }
            throw new Exception(string.Format("MarketTax was not expecting a market at level {0}", structure.Lvl));
        }

        static readonly int[] RateCrop = { 0, 100, 150, 220, 330, 500, 740, 1100, 1100, 1100, 1100 };
        static readonly int[] RateWood = { 0, 100, 150, 220, 330, 500, 740, 1100, 1100, 1100, 1100 };
        static readonly int[] RateGold = { 0, 0, 0, 0, 0, 100, 150, 220, 330, 500, 740 };
        static readonly int[] RateIron = { 0, 0, 0, 0, 0, 0, 0, 0, 200, 360, 660 };

        public static Resource HiddenResource(City city)
        {
            int maxbonus = city.Technologies.GetEffects(EffectCode.AtticStorageMod, EffectInheritance.SELF_ALL).DefaultIfEmpty(new Effect() { value = new object[] { 100 } }).Max(x => (int)x.value[0]);
            Resource resource = new Resource();
            foreach (Structure structure in city.Where(x => ObjectTypeFactory.IsStructureType("Basement", x)))
            {
                resource.Add(RateCrop[structure.Lvl], RateGold[structure.Lvl], RateIron[structure.Lvl], RateWood[structure.Lvl], 0);
            }
            return resource * maxbonus / 100;
        }

        internal static Resource GetActionCancelResource(DateTime beginTime, Resource cost) {
            if (DateTime.UtcNow.Subtract(beginTime).Seconds <= Config.actions_free_cancel_interval_in_sec) {
                return cost;
            }
            return new Resource(cost / 2);
        }
        
        internal static Resource GetSendCapacity(Structure structure) {
            int rate = structure.Lvl*200;
            return new Resource(rate, rate, rate, rate, rate);
        }
    }
}