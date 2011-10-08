#region

using System;
using System.Linq;
using Game.Data;
using Game.Setup;
using Game.Data.Tribe;
using Ninject;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        /// <summary>
        ///   Returns the cost for building the specified structure
        /// </summary>
        /// <param name = "city"></param>
        /// <param name = "type"></param>
        /// <param name = "lvl"></param>
        /// <returns></returns>
        public static Resource StructureCost(City city, uint type, byte lvl)
        {
            if (city.Battle == null)
                return Ioc.Kernel.Get<StructureFactory>().GetCost((int)type, lvl);

            return Ioc.Kernel.Get<StructureFactory>().GetCost((int)type, lvl)*Config.battle_cost_penalty;
        }

        /// <summary>
        ///   Returns the cost for training the specified unit
        /// </summary>
        /// <param name = "city"></param>
        /// <param name = "type"></param>
        /// <param name = "lvl"></param>
        /// <returns></returns>
        public static Resource UnitTrainCost(City city, ushort type, byte lvl)
        {
            if (city.Battle == null)
                return Ioc.Kernel.Get<UnitFactory>().GetCost(type, lvl);

            return Ioc.Kernel.Get<UnitFactory>().GetCost(type, lvl)*Config.battle_cost_penalty;
        }

        /// <summary>
        ///   Returns the cost for upgrading the specified unit
        /// </summary>
        /// <param name = "city"></param>
        /// <param name = "type"></param>
        /// <param name = "lvl"></param>
        /// <returns></returns>
        public static Resource UnitUpgradeCost(City city, ushort type, byte lvl)
        {
            if (city.Battle == null)
                return Ioc.Kernel.Get<UnitFactory>().GetUpgradeCost(type, lvl);

            return Ioc.Kernel.Get<UnitFactory>().GetUpgradeCost(type, lvl)*Config.battle_cost_penalty;
        }

        /// <summary>
        ///   Returns the amount of HP that can be repaired by the specified structure
        /// </summary>
        /// <param name = "structure"></param>
        /// <returns></returns>
        public static ushort RepairRate(Structure structure)
        {
            return (ushort)(structure.Stats.Labor*5);
        }

        /// <summary>
        ///   Returns the cost for repairing the specified amount
        /// </summary>
        /// <param name = "city"></param>
        /// <param name = "repairPower"></param>
        /// <returns></returns>
        public static Resource RepairCost(City city, ushort repairPower)
        {
            int lumber = repairPower;
            foreach (var effect in city.Technologies.GetEffects(EffectCode.RepairSaving, EffectInheritance.All))
                lumber -= repairPower*(int)effect.Value[0]/100;

            return new Resource(0, 0, 0, lumber, 0);
        }

        /// <summary>
        ///   Maximum amount of laborers that can be assigned at once
        /// </summary>
        /// <param name = "structure"></param>
        /// <returns></returns>
        public static ushort LaborMoveMax(Structure structure)
        {
            return (ushort)Math.Max(5, Math.Ceiling(structure.Stats.Base.MaxLabor/2.0));
        }

        /// <summary>
        /// </summary>
        /// <param name = "structure"></param>
        /// <returns></returns>
        public static double MarketTax(Structure structure)
        {
            double[] rate = {0.15, 0.15, 0.12, 0.09, 0.06, 0.03, 0, -0.03, -0.06, -0.09, -0.12};
            return rate[structure.Lvl];
        }

        /// <summary>
        ///   Gets amount of resources that are hidden
        /// </summary>
        /// <param name = "city"></param>
        /// <returns></returns>
        public static Resource HiddenResource(City city)
        {
            int[] rateCrop = {0, 100, 150, 220, 330, 500, 740, 1100, 1100, 1100, 1100};
            int[] rateWood = {0, 100, 150, 220, 330, 500, 740, 1100, 1100, 1100, 1100};
            int[] rateGold = {0, 0, 0, 0, 0, 100, 150, 220, 330, 500, 740};
            int[] rateIron = {0, 0, 0, 0, 0, 0, 0, 0, 200, 360, 660};

            int maxbonus =
                    city.Technologies.GetEffects(EffectCode.AtticStorageMod, EffectInheritance.SelfAll).DefaultIfEmpty(new Effect {Value = new object[] {100}}).
                            Max(x => (int)x.Value[0]);
            var resource = new Resource();
            foreach (var structure in city.Where(x => Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Basement", x)))
                resource.Add(rateCrop[structure.Lvl], rateGold[structure.Lvl], rateIron[structure.Lvl], rateWood[structure.Lvl], 0);
            return resource*maxbonus/100;
        }

        /// <summary>
        ///   Returns the amount of resources the user will get back when cancelling an action
        /// </summary>
        /// <param name = "beginTime"></param>
        /// <param name = "cost"></param>
        /// <returns></returns>
        internal static Resource GetActionCancelResource(DateTime beginTime, Resource cost)
        {
            if (DateTime.UtcNow.Subtract(beginTime).Seconds <= Config.actions_free_cancel_interval_in_sec)
                return cost;
            return new Resource(cost/2);
        }

        internal static Resource GetSendCapacity(Structure structure)
        {
            //Maximum amount of resources that can be sent from the specified structure        
            int[] sendRate = {0, 200, 200, 400, 400, 600, 600, 800, 1000, 1200, 1200, 1400, 1600, 1800, 1800, 2000};
            return new Resource(sendRate[structure.Lvl], sendRate[structure.Lvl], sendRate[structure.Lvl], sendRate[structure.Lvl], sendRate[structure.Lvl]);
        }

        internal static int GetCropRate(City city)
        {
            double[] lvlBonus = {1, 1, 1, 1, 1, 1, 1, 1.1, 1.1, 1.2, 1.2, 1.3, 1.3, 1.4, 1.4, 1.5};
            return (int)city.Sum(x => Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Crop", x) ? x.Stats.Labor*lvlBonus[x.Lvl] : 0) + Config.resource_base_bonus;
        }

        public static ushort CalculateCityValue(City city)
        {
            return (ushort)city.Sum(x => x.Lvl);
        }

        internal static int GetWeaponExportLaborProduce(int weaponExport, int labor) {
            switch(weaponExport) 
            {
                case 1:
                case 2:
                    return labor / 3;
                case 3:
                case 4:
                    return labor * 2 / 5;
                case 5:
                    return labor / 2;
                default:
                    return 0;
            }
        }


        public static Resource GetTribeUpgradeCost(byte level)
        {
            return new Resource(5000, 2000, 200, 10000, 0) * Tribe.MEMBERS_PER_LEVEL * level;
        }
    }
}