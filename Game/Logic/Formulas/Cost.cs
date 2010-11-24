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

            return StructureFactory.GetCost((int)type, lvl) * Config.battle_cost_penalty;
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

            return UnitFactory.GetCost(type, lvl) * Config.battle_cost_penalty;
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

            return UnitFactory.GetUpgradeCost(type, lvl) * Config.battle_cost_penalty;
        }

        /// <summary>
        /// Returns the amount of HP that can be repaired by the specified structure
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public static ushort RepairRate(Structure structure)
        {
            return (ushort)(structure.Stats.Labor * 5);
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
        /// <param name="city"></param>
        /// <returns></returns>
        public static int GetLaborRate(int laborTotal, City city) {
            if (laborTotal < 140)
                laborTotal = 140;

            if (city.Technologies.GetEffects(EffectCode.CountEffect, EffectInheritance.SELF_ALL).Any(x => (int) x.value[0] == 30021))
                return (int)((43200 / (-6.845 * Math.Log(laborTotal) + 55)) * 0.7);           

            return (int)(43200 / (-6.845 * Math.Log(laborTotal) + 55));
        }

        /// <summary>
        /// Maximum amount of laborers that can be assigned at once
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public static ushort LaborMoveMax(Structure structure)
        {
            return (ushort)Math.Max(5, Math.Ceiling(structure.Stats.Base.MaxLabor / 10.0f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public static double MarketTax(Structure structure)
        {
            double[] rate = { 0, .15, .12, .09, .06, .03, 0, 0.03, .06, .09 };
            return rate[structure.Lvl];
        }

        /// <summary>
        /// Gets amount of resources that are hidden
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public static Resource HiddenResource(City city)
        {
            int[] rateCrop = { 0, 100, 150, 220, 330, 500, 740, 1100, 1100, 1100, 1100 };
            int[] rateWood = { 0, 100, 150, 220, 330, 500, 740, 1100, 1100, 1100, 1100 };
            int[] rateGold = { 0, 0, 0, 0, 0, 100, 150, 220, 330, 500, 740 };
            int[] rateIron = { 0, 0, 0, 0, 0, 0, 0, 0, 200, 360, 660 };

            int maxbonus = city.Technologies.GetEffects(EffectCode.AtticStorageMod, EffectInheritance.SELF_ALL).DefaultIfEmpty(new Effect { value = new object[] { 100 } }).Max(x => (int)x.value[0]);
            Resource resource = new Resource();
            foreach (Structure structure in city.Where(x => ObjectTypeFactory.IsStructureType("Basement", x)))
            {
                resource.Add(rateCrop[structure.Lvl], rateGold[structure.Lvl], rateIron[structure.Lvl], rateWood[structure.Lvl], 0);
            }
            return resource * maxbonus / 100;
        }

        /// <summary>
        /// Returns the amount of resources the user will get back when cancelling an action
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        internal static Resource GetActionCancelResource(DateTime beginTime, Resource cost) {
            if (DateTime.UtcNow.Subtract(beginTime).Seconds <= Config.actions_free_cancel_interval_in_sec) {
                return cost;
            }
            return new Resource(cost / 2);
        }
        
        /// <summary>
        /// Maximum amount of resources that can be sent from the specified structure
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        static int[] sendRate = { 0, 200, 200, 400, 400, 600, 600, 800, 1000, 1200, 1200, 1400, 1600, 1800, 1800, 2000 };
        internal static Resource GetSendCapacity(Structure structure) {
            return new Resource(sendRate[structure.Lvl], sendRate[structure.Lvl], sendRate[structure.Lvl], sendRate[structure.Lvl], sendRate[structure.Lvl]);
        }

        internal static int GetCropRate(City city) {
            double[] lvlBonus = { 1, 1, 1, 1, 1, 1, 1, 1.1, 1.1, 1.2, 1.2, 1.3, 1.3, 1.4, 1.4, 1.5 };
            return (int)city.Sum(x => ObjectTypeFactory.IsStructureType("Crop", x) ? x.Stats.Labor * lvlBonus[x.Lvl] : 0);
        }
    }
}