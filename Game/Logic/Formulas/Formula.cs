#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Setup;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        [Obsolete("Used for testing only", true)]
        public Formula()
        {
        }

        public Formula(ObjectTypeFactory objectTypeFactory, UnitFactory unitFactory, StructureFactory structureFactory)
        {
            ObjectTypeFactory = objectTypeFactory;
            UnitFactory = unitFactory;
            StructureFactory = structureFactory;
        }

        public static Formula Current { get; set; }

        public ObjectTypeFactory ObjectTypeFactory { get; set; }

        public UnitFactory UnitFactory { get; set; }

        public StructureFactory StructureFactory { get; set; }

        /// <summary>
        ///     Applies the specified effects to the specified radius. This is used by AwayFromLayout for building validation.
        /// </summary>
        /// <param name="effects"></param>
        /// <param name="radius"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual int GetAwayFromRadius(IEnumerable<Effect> effects, byte radius, ushort type)
        {
            return radius +
                   effects.DefaultIfEmpty()
                          .Min(
                               x =>
                               (x != null && x.Id == EffectCode.AwayFromStructureMod && (int)x.Value[0] == type)
                                       ? (int)x.Value[1]
                                       : 0);
        }

        /// <summary>
        ///     Returns maximum number of concurrent build/upgrades allowed for the given structure level
        /// </summary>
        /// <param name="mainstructureLevel"></param>
        /// <returns></returns>
        public virtual int ConcurrentBuildUpgrades(int mainstructureLevel)
        {
            return mainstructureLevel >= 11 ? 3 : 2;
        }

        /// <summary>
        ///     Returns the crop cap based on the main building level
        /// </summary>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public virtual int ResourceCropCap(byte lvl)
        {
            int[] cap =
            {
                    700, 700, 880, 1130, 1450, 1880, 2440, 3200, 4200, 5500, 7200, 9500, 12500, 16500, 21800, 25000
            };
            return cap[lvl];
        }

        /// <summary>
        ///     /// Returns the wood cap based on the main building level
        /// </summary>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public virtual int ResourceWoodCap(byte lvl)
        {
            int[] cap =
            {
                    700, 700, 880, 1130, 1450, 1880, 2440, 3200, 4200, 5500, 7200, 9500, 12500, 16500, 21800, 25000
            };
            return cap[lvl];
        }

        /// <summary>
        ///     Returns the iron cap based on the main building level
        /// </summary>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public virtual int ResourceIronCap(byte lvl)
        {
            int[] cap = {100, 100, 100, 100, 100, 100, 170, 380, 620, 900, 1240, 1630, 2090, 2620, 3260, 4000};
            return cap[lvl];
        }

        /// <summary>
        ///     Returns the amount of iron the user should get for the specified city
        /// </summary>
        /// <param name="city">City to recalculate resources for.</param>
        /// <returns></returns>
        public virtual int GetIronRate(ICity city)
        {
            int[] multiplier = {int.MaxValue, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 5, 5, 5, 5, 4};

            return city.Sum(x => ObjectTypeFactory.IsStructureType("Iron", x) ? x.Stats.Labor / multiplier[x.Lvl] : 0);
        }

        /// <summary>
        ///     Returns the amount of crop the user should get for the specified city
        /// </summary>
        /// <param name="city">City to recalculate resources for</param>
        /// <returns></returns>
        public virtual int GetCropRate(ICity city)
        {
            double[] lvlBonus = {1, 1, 1, 1, 1, 1, 1, 1.1, 1.1, 1.2, 1.2, 1.3, 1.3, 1.4, 1.4, 1.5};
            return 40 + city.Lvl * 5 +
                   (int)
                   city.Sum(x => ObjectTypeFactory.IsStructureType("Crop", x) ? x.Stats.Labor * lvlBonus[x.Lvl] : 0);
        }

        /// <summary>
        ///     Returns the amount of wood the user should get for the specified city.
        ///     Notice: This function looks at all the Forest Camps Rate property and adds them up.
        ///     It doesn't actually go into the Forest to recalculate the values. See <see cref="GetWoodRateForForest" /> for
        ///     a calculation that factors the Forest.
        /// </summary>
        /// <param name="city">City to recalculate resources for</param>
        /// <returns></returns>
        public virtual int GetWoodRate(ICity city)
        {
            return 40 + city.Lvl * 5 + city.Sum(x =>
                {
                    object rate;
                    if (!ObjectTypeFactory.IsStructureType("ForestCamp", x) || x.Lvl == 0 ||
                        !x.Properties.TryGet("Rate", out rate))
                    {
                        return 0;
                    }

                    return (int)rate;
                });
        }

        /// <summary>
        ///     Returns the amount of gold the user should get for the specified city
        /// </summary>
        /// <param name="city">City to recalculate resources for</param>
        /// <returns></returns>
        public virtual int GetGoldRate(ICity city)
        {
            int value = 0;
            foreach (Structure structure in city.Where(x => ObjectTypeFactory.IsStructureType("Market", x)))
            {
                if (structure.Lvl >= 10)
                {
                    value += 50;
                }
                else if (structure.Lvl >= 6)
                {
                    value += 18;
                }
                else if (structure.Lvl >= 4)
                {
                    value += 4;
                }
            }
            return value;
        }

        /// <summary>
        ///     Returns the rate that the specified structure should gather from the given forest.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="stats"></param>
        /// <param name="efficiency"></param>
        /// <returns></returns>
        public virtual int GetWoodRateForForest(Forest forest, StructureStats stats, double efficiency)
        {
            return (int)(stats.Labor * forest.Rate * (1d + efficiency));
        }

        /// <summary>
        ///     Returns the highest X for 1 count available or 0 if none is found
        /// </summary>
        /// <param name="tech"></param>
        /// <returns></returns>
        public virtual int GetXForOneCount(ITechnologyManager tech)
        {
            var effects = tech.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible);

            if (effects.Count == 0)
            {
                return ushort.MaxValue;
            }

            return effects.Min(x => (int)x.Value[0]);
        }

        public virtual Resource GetInitialCityResources()
        {
            return new Resource(700, 0, 0, 700, 35);
        }

        public virtual byte GetInitialCityRadius()
        {
            return 4;
        }

        public virtual decimal GetInitialAp()
        {
            return 50m;
        }

        public virtual int GetGateLimit(byte level)
        {
            if (Config.stronghold_gate_limit > 0)
            {
                return Config.stronghold_gate_limit;
            }
            return level * 10000;
        }

        public virtual Resource GetGateRepairCost(byte level, decimal damagedGateHp)
        {
            return new Resource(0,
                                (int)(level * damagedGateHp / 8),
                                (int)(level * damagedGateHp / 16),
                                (int)(level * damagedGateHp / 4),
                                0);
        }

        public virtual int GetMainBattleMeter(byte level)
        {
            if (Config.stronghold_battle_meter > 0)
            {
                return Config.stronghold_battle_meter;
            }
            return level * 1000;
        }
    }
}