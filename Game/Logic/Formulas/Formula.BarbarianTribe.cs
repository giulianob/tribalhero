using System;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Data;

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        private SlotMachineCalculator barbarianTribeCropCalculator;
        private SlotMachineCalculator barbarianTribeWoodCalculator;
        private SlotMachineCalculator barbarianTribeIronCalculator;
        private SlotMachineCalculator barbarianTribeGoldCalculator;

        public Resource BarbarianTribeResources(byte level)
        {
            barbarianTribeCropCalculator = barbarianTribeCropCalculator ??
                                           new SlotMachineCalculator(new[] {92, 208, 461, 864, 1423, 2142, 3027, 4081, 5308, 6709},
                                                                     new[] {138, 311, 692, 1296, 2134, 3213, 4540, 6122, 7961, 10064},
                                                                     new[] {1.0, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                                                     new Random());

            barbarianTribeWoodCalculator = barbarianTribeWoodCalculator ??
                                           new SlotMachineCalculator(new[] {92, 208, 461, 864, 1423, 2142, 3027, 4081, 5308, 6709},
                                                                     new[] {138, 311, 692, 1296, 2134, 3213, 4540, 6122, 7961, 10064},
                                                                     new[] {1.0, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                                                     new Random());

            barbarianTribeGoldCalculator = barbarianTribeGoldCalculator ??
                                           new SlotMachineCalculator(new[] {1, 1, 1, 207, 279, 314, 264, 498, 578, 531},
                                                                     new[] {50, 75, 100, 257, 379, 464, 464, 748, 888, 921},
                                                                     new[] {0.01, 0.05, 0.1, 0.25, 0.35, 0.45, 0.55, 0.65, 0.75, 0.95},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                                                     new Random());

            barbarianTribeIronCalculator = barbarianTribeIronCalculator ??
                                           new SlotMachineCalculator(new[] {1, 1, 1, 1, 1, 18, 16, 27, 40, 65},
                                                                     new[] {20, 30, 50, 50, 50, 28, 32, 57, 80, 135},
                                                                     new[] {0.0001, 0.001, 0.01, 0.05, 0.1, 0.3, 0.5, 0.6, 0.75, 0.9},
                                                                     new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                                                     new Random());

            return new Resource(barbarianTribeCropCalculator.Roll(level),
                                barbarianTribeGoldCalculator.Roll(level),
                                barbarianTribeIronCalculator.Roll(level),
                                barbarianTribeWoodCalculator.Roll(level));
        }

        public void BarbarianTribeUpkeep(byte level, out int upkeep, out byte unitLevel)
        {
            int[] btUnitLevel = new[] {1, 1, 1, 2, 2, 3, 3, 4, 5, 6};
            int[] btUnitUpkeep = new[] {10, 20, 41, 73, 117, 171, 237, 313, 401, 500};

            unitLevel = (byte)btUnitLevel[level-1];
            upkeep = btUnitUpkeep[level-1];
        }

        public double[,] BarbarianTribeUnitRatios()
        {
            return new[,] {
                {1, 0, 0, 0, 0, 0 },
                {0.9, 0.1, 0, 0, 0, 0 },
                {0.5, 0.5, 0, 0, 0, 0 },
                {0.5, 0.25, 0.25, 0, 0, 0 },
                {0.5, 0.25, 0.25, 0, 0, 0 },
                {0.4, 0.25, 0.25, 0.1, 0, 0 },
                {0.4, 0.25, 0.25, 0.1, 0, 0 },
                {0.35, 0.2, 0.25, 0.15, 0.5, 0 },
                {0.2, 0.2, 0.25, 0.15, 0.15, 0.05 },
                {0.25, 0.2, 0.25, 0.15, 0.15, 0.1 },
            };
        }

        public ushort[] BarbarianTribeUnitTypes()
        {
            return new ushort[] {101, 102, 105, 103, 104, 106};
        }

        public Resource BarbarianTribeBonus(byte level, IBattleManager battle, ICombatGroup combatGroup)
        {
            var bonusAmt = new[] {15, 34, 75, 141, 232, 349, 493, 665, 865, 1094};

            // Get nothing if they didnt defeat the camp
            if (battle.Defenders.Upkeep > 0)
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

            return new Resource(wood: bonusAmt[level - 1], crop: bonusAmt[level - 1]) * (double)(myTotal / total);
        }
    }
}
