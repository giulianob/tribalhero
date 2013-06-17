#region

using System;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Data.Forest;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        public virtual int SendTime(IStructure structure, int distance)
        {
            return (int)(MoveTime(11) * distance * 100 /
                         (100 +
                          structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self)
                                  .DefaultIfEmpty()
                                  .Max(x => x == null ? 0 : (int)x.Value[0])));
        }

        public virtual int TradeTime(IStructure structure, Resource resource)
        {
            return TradeTime(structure, 0);
        }

        public virtual int TradeTime(IStructure structure, int quantity)
        {
            return 14400 * 100 /
                   (100 +
                    structure.Technologies.GetEffects(EffectCode.TradeSpeedMod, EffectInheritance.Self)
                             .DefaultIfEmpty()
                             .Max(x => x == null ? 0 : (int)x.Value[0]));
        }

        public virtual int LaborMoveTime(IStructure structure, ushort count, bool cityToStructure)
        {
            const int secondsPerLaborer = 180;

            int totalLaborers = structure.City.GetTotalLaborers();
            int moveTime;
            if (cityToStructure && totalLaborers < 160)
            {
                moveTime = (int)Math.Ceiling(0.95 * Math.Exp(0.033 * totalLaborers)) * count;
            }
            else
            {
                var effects = structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod);
                int overtime = 0;
                if (effects.Count > 0)
                {
                    overtime = effects.Max(x => (int)x.Value[0]);
                }

                moveTime = (100 - overtime * 10) * count * secondsPerLaborer / 100;
            }

            if (!cityToStructure)
            {
                moveTime = moveTime / 20;
            }

            return moveTime;
        }

        public virtual int TrainTime(int structureLvl, int unitCount, IBaseUnitStats stats, ICity city, ITechnologyManager techManager)
        {
            int[] structureDiscountByLevel = new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 15, 20, 30, 40};
            
            var currentCityUpkeep = city.Troops.Upkeep;
            var structureLevelDiscount = structureDiscountByLevel[Math.Min(structureLvl, structureDiscountByLevel.Length - 1)].FromPercentageDiscount();
            var trainTimePerUnit = stats.BuildTime * structureLevelDiscount;

            if (currentCityUpkeep < 15)
            {
                var trainFirst15Discount = techManager.GetEffects(EffectCode.UnitTrainTimeFirst15Reduction)
                                                      .DefaultIfEmpty()
                                                      .Max(e => e == null ? 0 : (int)e.Value[0])
                                                      .FromPercentageDiscount();

                var discountedUnits = Math.Min(15 - currentCityUpkeep, unitCount);
                return (int)((trainTimePerUnit * trainFirst15Discount * discountedUnits) + (trainTimePerUnit * (unitCount - discountedUnits)));
            }
            
            return (int)(trainTimePerUnit * unitCount);
        }

        public virtual int BuildTime(int baseValue, ICity city, ITechnologyManager em)
        {
            IStructure university = city.FirstOrDefault(structure => ObjectTypeFactory.IsStructureType("University", structure));
            return (int)(baseValue * (100 - (university == null ? 0 : university.Stats.Labor) * 0.25) / 100);
        }

        /// <summary>
        ///     Get number of seconds it takes to move object by 1 tile
        /// </summary>
        /// <param name="speed">Objects speed</param>
        /// <returns></returns>
        public virtual decimal MoveTime(decimal speed)
        {
            // 60 is second per square, 12 is the average speed for all troops
            // second per square is lowered from 80 to 60. 3/9/2011
            // Base speed is 45 tile/hr, Each speed is extra +5 tile/hr 2/10/2013
            return Math.Round(3600 / (45 + speed * 5), 1);
        }

        public virtual int MoveTimeTotal(ITroopStub stub, int distance, bool isAttacking)
        {
            var moveTime = MoveTime(stub.Speed);
            decimal doubleTimeBonus = 0;
            decimal rushBonus = 0;
            int doubleTimeDistance = 500;

            foreach (var effect in stub.City.Technologies.GetEffects(EffectCode.TroopSpeedMod))
            {
                // Getting rush attack/defense bonus;
                if ((((string)effect.Value[1]).ToUpper() == "ATTACK" && isAttacking) ||
                    (((string)effect.Value[1]).ToUpper() == "DEFENSE" && !isAttacking))
                {
                    rushBonus += (int)effect.Value[0];
                }
                // Getting double time bonus
                else if (((string)effect.Value[1]).ToUpper() == "DISTANCE")
                {
                    doubleTimeBonus += (int)effect.Value[0];
                }
            }

            var rushBonusPercentage = rushBonus / 100;
            var doubleTimeBonusPercentage =  doubleTimeBonus / 100;

            if (distance <= doubleTimeDistance)
            {
                return (int)(moveTime * distance / (1 + rushBonusPercentage) * (decimal)Config.seconds_per_unit);
            }

            var shortDistance = moveTime * doubleTimeDistance / (1 + rushBonusPercentage);
            var longDistance = (distance - doubleTimeDistance) * moveTime / (1 + rushBonusPercentage + doubleTimeBonusPercentage);
            return (int)((shortDistance + longDistance) * (decimal)Config.seconds_per_unit);
        }

        /// <summary>
        ///     Returns number of seconds it takes to get 1 labor
        /// </summary>
        /// <param name="laborTotal"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public virtual int GetLaborRate(int laborTotal, ICity city)
        {
            if (laborTotal < 140)
            {
                laborTotal = 140;
            }

            var effects = city.Technologies.GetEffects(EffectCode.LaborTrainTimeMod, EffectInheritance.SelfAll);
            double rateBonus = 1;
            if (effects.Count > 0)
            {
                rateBonus = (effects.Min(x => (int)x.Value[0]) * 10) / 100f;
                if (effects.Count > 1)
                {
                    rateBonus *= Math.Pow(0.92, effects.Count - 1); // for every extra tribal gathering, you gain 8 % each
                }
            }
            double newMultiplier = Math.Min(2, 3 - (double)laborTotal / 400);
            newMultiplier = Math.Max(1, newMultiplier);
            return (int)((43200 / ((-6.845 * Math.Log(laborTotal / 1.3 - 100) + 55) * newMultiplier)) * rateBonus * Config.seconds_per_unit);
        }

        public virtual TimeSpan ReadCsvTimeFormat(string time)
        {
            if (time.StartsWith("!"))
            {
                return TimeSpan.FromSeconds(int.Parse(time.Remove(0, 1)) / Config.seconds_per_unit);
            }

            return TimeSpan.FromSeconds(int.Parse(time));
        }

        public virtual double GetBattleInterval(ICombatList defenders, ICombatList attackers)
        {
            var count = defenders.SelectMany(o => o).Count() + attackers.SelectMany(o => o).Count();
            // at 400 objects, the reduction is cap'ed at 20% of the original speed.
            var ret = Config.battle_turn_interval * 100 / (100 + Math.Min(500, count));
            return Config.server_production ? Math.Max(4, ret) : ret;
        }

        public double GetLumbermillCampBuildTime(int campBuildTime, IStructure lumbermill, IForest forest, TileLocator tileLocator)
        {
            var distance = tileLocator.TileDistance(lumbermill.X, lumbermill.Y, forest.X, forest.Y);
            return BuildTime(campBuildTime, lumbermill.City, lumbermill.City.Technologies) + distance * 5;
        }
    }
}