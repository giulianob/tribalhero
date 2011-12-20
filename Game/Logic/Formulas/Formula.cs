#region

using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Actions;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        /// <summary>
        ///   Applies the specified effects to the specified radius. This is used by AwayFromLayout for building validation.
        /// </summary>
        /// <param name = "effects"></param>
        /// <param name = "radius"></param>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static int GetAwayFromRadius(IEnumerable<Effect> effects, byte radius, ushort type)
        {
            return radius + effects.DefaultIfEmpty().Min(x => (x!=null && x.Id == EffectCode.AwayFromStructureMod && (int)x.Value[0] == type) ? (int)x.Value[1] : 0);
        }

        /// <summary>
        /// Returns maximum number of concurrent build/upgrades allowed for the given structure level
        /// </summary>
        /// <param name="mainstructureLevel"></param>
        /// <returns></returns>
        public static int ConcurrentBuildUpgrades(int mainstructureLevel)
        {
            return mainstructureLevel >= 11 ? 3 : 2;
        }

        /// <summary>
        ///   Returns the crop cap based on the main building level
        /// </summary>
        /// <param name = "lvl"></param>
        /// <returns></returns>
        public static int ResourceCropCap(byte lvl)
        {
            int[] cap = { 700, 700, 880, 1130, 1450, 1880, 2440, 3200, 4200, 5500, 7200, 9500, 12500, 16500, 21800, 25000 };
            return cap[lvl];
        }

        /// <summary>
        ///   /// Returns the wood cap based on the main building level
        /// </summary>
        /// <param name = "lvl"></param>
        /// <returns></returns>
        public static int ResourceWoodCap(byte lvl)
        {
            int[] cap = { 700, 700, 880, 1130, 1450, 1880, 2440, 3200, 4200, 5500, 7200, 9500, 12500, 16500, 21800, 25000 };
            return cap[lvl];
        }

        /// <summary>
        ///   Returns the iron cap based on the main building level
        /// </summary>
        /// <param name = "lvl"></param>
        /// <returns></returns>
        public static int ResourceIronCap(byte lvl)
        {
            int[] cap = { 100, 100, 100, 100, 100, 100, 170, 380, 620, 900, 1240, 1630, 2090, 2620, 3260, 4000 };
            return cap[lvl];
        }

        /// <summary>
        ///   Returns the amount of iron the user should get for the specified city
        /// </summary>
        /// <param name="city">City to recalculate resources for.</param>
        /// <returns></returns>
        public static int GetIronRate(City city)
        {
            int[] multiplier = { int.MaxValue, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 5, 5, 5, 5, 4 };

            return city.Sum(x => Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Iron", x) ? x.Stats.Labor / multiplier[x.Lvl] : 0);
        }

        /// <summary>
        /// Returns the amount of crop the user should get for the specified city
        /// </summary>
        /// <param name="city">City to recalculate resources for</param>
        /// <returns></returns>
        public static int GetCropRate(City city)
        {
            double[] lvlBonus = { 1, 1, 1, 1, 1, 1, 1, 1.1, 1.1, 1.2, 1.2, 1.3, 1.3, 1.4, 1.4, 1.5 };
            return 40 + city.Lvl * 5 + (int)city.Sum(x => Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Crop", x) ? x.Stats.Labor * lvlBonus[x.Lvl] : 0);
        }

        /// <summary>
        /// Returns the amount of wood the user should get for the specified city.
        /// Notice: This function looks at all the Forest Camps Rate property and adds them up. 
        /// It doesn't actually go into the Forest to recalculate the values. See <see cref="GetWoodRateForForest"/> for
        /// a calculation that factors the Forest.
        /// </summary>
        /// <param name="city">City to recalculate resources for</param>
        /// <returns></returns>
        public static int GetWoodRate(City city)
        {
            return 40 + city.Lvl * 5 + city.Sum(x =>
            {
                object rate;
                if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("ForestCamp", x) || x.Lvl == 0 || !x.Properties.TryGet("Rate", out rate))
                    return 0;

                return (int)rate;
            });
        }

        /// <summary>
        /// Returns the rate that the specified structure should gather from the given forest.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="stats"></param>
        /// <param name="efficiency"></param>
        /// <returns></returns>
        public static int GetWoodRateForForest(Forest forest, StructureStats stats, double efficiency)
        {
            return (int)(stats.Labor * forest.Rate * (1d + efficiency));
        }

        /// <summary>
        ///   Returns the highest X for 1 count available or 0 if none is found
        /// </summary>
        /// <param name = "tech"></param>
        /// <returns></returns>
        public static int GetXForOneCount(TechnologyManager tech)
        {
            var effects = tech.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible);

            if (effects.Count == 0)
                return ushort.MaxValue;

            return effects.Min(x => (int)x.Value[0]);
        }

        public static Resource GetInitialCityResources()
        {
            return new Resource(700, 0, 0, 700, 35);
        }

        public static byte GetInitialCityRadius()
        {
            return 4;
        }
    }
}