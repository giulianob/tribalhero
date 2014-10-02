using System;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Setup;

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        private SlotMachineCalculator barbarianTribeCropCalculator;
        private SlotMachineCalculator barbarianTribeWoodCalculator;
        private SlotMachineCalculator barbarianTribeIronCalculator;
        private SlotMachineCalculator barbarianTribeGoldCalculator;

        public virtual Resource BarbarianTribeResources(IBarbarianTribe barbarianTribe)
        {
            int seed;
            unchecked
            {
                seed = (int)(barbarianTribe.ObjectId +
                             barbarianTribe.PrimaryPosition.X +
                             barbarianTribe.PrimaryPosition.Y +
                             2 ^ barbarianTribe.CampRemains);
            }

            var rand = new Random(seed);

            barbarianTribeCropCalculator = barbarianTribeCropCalculator ??
                                           new SlotMachineCalculator(new[] {92, 208, 461, 864, 1423, 2142, 3027, 4081, 5308, 6709},
                                                                     new[] {138, 311, 692, 1296, 2134, 3213, 4540, 6122, 7961, 10064},
                                                                     new[] {1.0, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

            barbarianTribeWoodCalculator = barbarianTribeWoodCalculator ??
                                           new SlotMachineCalculator(new[] {92, 208, 461, 864, 1423, 2142, 3027, 4081, 5308, 6709},
                                                                     new[] {138, 311, 692, 1296, 2134, 3213, 4540, 6122, 7961, 10064},
                                                                     new[] {1.0, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

            barbarianTribeGoldCalculator = barbarianTribeGoldCalculator ??
                                           new SlotMachineCalculator(new[] {1, 1, 1, 207, 279, 314, 264, 498, 578, 531},
                                                                     new[] {50, 75, 100, 257, 379, 464, 464, 748, 888, 921},
                                                                     new[] {0.01, 0.05, 0.1, 0.25, 0.35, 0.45, 0.55, 0.65, 0.75, 0.95},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

            barbarianTribeIronCalculator = barbarianTribeIronCalculator ??
                                           new SlotMachineCalculator(new[] {1, 1, 1, 1, 1, 18, 16, 27, 40, 65},
                                                                     new[] {20, 30, 50, 50, 50, 28, 32, 57, 80, 135},
                                                                     new[] {0.0001, 0.001, 0.01, 0.05, 0.1, 0.3, 0.5, 0.6, 0.75, 0.9},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

            return new Resource(barbarianTribeCropCalculator.Roll(barbarianTribe.Lvl, rand),
                                barbarianTribeGoldCalculator.Roll(barbarianTribe.Lvl, rand),
                                barbarianTribeIronCalculator.Roll(barbarianTribe.Lvl, rand),
                                barbarianTribeWoodCalculator.Roll(barbarianTribe.Lvl, rand));
        }

        public virtual void BarbarianTribeUpkeep(byte level, out int upkeep, out byte unitLevel)
        {
            int[] btUnitLevel = {1, 1, 1, 2, 2, 3, 3, 4, 5, 6};
            int[] btUnitUpkeep = {5, 20, 41, 73, 117, 171, 237, 313, 401, 500};

            unitLevel = (byte)btUnitLevel[level-1];
            upkeep = btUnitUpkeep[level-1];
        }

        public virtual double[][] BarbarianTribeUnitRatios()
        {
            return new[]
            {
                new[] {1.0, 0, 0, 0, 0, 0 },
                new[] {0.9, 0.1, 0, 0, 0, 0 },
                new[] {0.5, 0.5, 0, 0, 0, 0 },
                new[] {0.5, 0.25, 0.25, 0, 0, 0 },
                new[] {0.5, 0.25, 0.25, 0, 0, 0 },
                new[] {0.4, 0.25, 0.25, 0.1, 0, 0 },
                new[] {0.4, 0.25, 0.25, 0.1, 0, 0 },
                new[] {0.35, 0.2, 0.25, 0.15, 0.05, 0 },
                new[] {0.2, 0.2, 0.25, 0.15, 0.15, 0.05 },
                new[] {0.25, 0.2, 0.25, 0.15, 0.15, 0.1 },
            };
        }

        public virtual ushort[] BarbarianTribeUnitTypes()
        {
            return new ushort[] {101, 102, 105, 103, 104, 106};
        }

        public virtual Resource BarbarianTribeBonus(byte level, IBattleManager battle, ICombatGroup combatGroup, IBarbarianTribe barbarianTribe)
        {
            var bonusAmt = new[] {15, 34, 75, 141, 232, 349, 493, 665, 865, 1094};

            // Get nothing if they didnt defeat the camp
            if (battle.Defenders.UpkeepExcludingWaitingToJoinBattle > 0)
            {
                return new Resource();
            }            
            
            decimal total =
                    battle.Attackers.AllCombatObjects()
                          .Sum(combatObj => combatObj.Upkeep * combatObj.RoundsParticipated * combatObj.RoundsParticipated);

            decimal myTotal = combatGroup.Sum(combatObj => combatObj.Upkeep * combatObj.RoundsParticipated * combatObj.RoundsParticipated);

            if (total == 0 || myTotal == 0)
            {
                return new Resource();
            }

            var myPercentage = myTotal / total;

            var bonus = new Resource(wood: bonusAmt[level - 1], crop: bonusAmt[level - 1]) * (double)myPercentage;

            // Add remaining barb resources as well
            bonus.Add(barbarianTribe.Resource * (double)myPercentage);

            return bonus;
        }
    }
}
