using System;
using Game.Data;
using Game.Data.Stronghold;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        private static readonly double GateScaling = Math.Pow(2d, 1d / 500d);

        private static readonly double MeterScaling = Math.Pow(2d, 3d / 1000d);

        public virtual decimal StrongholdGateLimit(byte level)
        {
            if (Config.stronghold_gate_limit > 0)
            {
                return Config.stronghold_gate_limit;
            }

            int hoursSinceScalingStarted = Math.Min(1000, GetHoursSinceServerStarted());

            if (hoursSinceScalingStarted > 0)
            {
                return (decimal)(Math.Round(Math.Pow(GateScaling, hoursSinceScalingStarted) * StrongholdMainBattleMeterBase(level)) * 10);

            }

            return StrongholdMainBattleMeterBase(level) * 10;
        }

        private int GetHoursSinceServerStarted()
        {
            return Convert.ToInt32(SystemClock.Now.Subtract((DateTime)SystemVariableManager["Server.date"].Value).TotalHours) - 30 * 24;
        }

        public virtual int StrongholdGateHealHp(StrongholdState state, byte level)
        {
            decimal hp = StrongholdGateLimit(level);
            if (state != StrongholdState.Neutral)
            {
                hp /= 2m;
            }

            return (int)hp;
        }

        public virtual Resource StrongholdGateRepairCost(decimal damagedGateHp)
        {
            return new Resource(0, (int)(damagedGateHp / 8), (int)(damagedGateHp / 16), (int)(damagedGateHp / 4));
        }

        private int StrongholdMainBattleMeterBase(byte level)
        {
            int[] meters = { 0, 500, 600, 700, 800, 950, 1100, 1250, 1450, 1650, 1850, 2100, 2350, 2600, 2900, 3200, 3500, 3850, 4200, 4600, 5000 };
            return meters[level];
        }        

        public virtual int StrongholdMainBattleMeter(byte level)
        {
            if (Config.stronghold_battle_meter > 0)
            {
                return Config.stronghold_battle_meter;
            }

            int hoursSinceScalingStarted = Math.Min(1000, GetHoursSinceServerStarted());

            if (hoursSinceScalingStarted > 0)
            {
                return (int)Math.Round(Math.Pow(MeterScaling, hoursSinceScalingStarted) * StrongholdMainBattleMeterBase(level));

            }

            return StrongholdMainBattleMeterBase(level);
        }

        public virtual void StrongholdUpkeep(byte level, out int upkeep, out byte unitLevel)
        {
            int[] unitLevels = {1, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10};

            upkeep = Config.stronghold_fixed_upkeep == 0 ? (int)(StrongholdMainBattleMeter(level) * .8) : Config.stronghold_fixed_upkeep;
            unitLevel = (byte)unitLevels[level-1];
        }

        public virtual double[][] StrongholdUnitRatio()
        {
            return new[]
            {
                new[] {1.00, .00, .00, .00, .00, .00, .00, .00,},
                new[] {.90, .10, .00, .00, .00, .00, .00, .00,},
                new[] {.50, .50, .00, .00, .00, .00, .00, .00,},
                new[] {.50, .25, .25, .00, .00, .00, .00, .00,},
                new[] {.50, .25, .25, .00, .00, .00, .00, .00,},
                new[] {.40, .25, .25, .10, .00, .00, .00, .00,},
                new[] {.40, .25, .25, .10, .00, .00, .00, .00,},
                new[] {.35, .20, .25, .15, .05, .00, .00, .00,},
                new[] {.25, .20, .25, .15, .15, .00, .00, .00,},
                new[] {.25, .20, .25, .15, .15, .00, .00, .00,},
                new[] {.20, .15, .20, .15, .15, .15, .00, .00,},
                new[] {.20, .15, .20, .15, .15, .15, .00, .00,},
                new[] {.15, .10, .15, .15, .25, .20, .00, .00,},
                new[] {.10, .10, .10, .15, .25, .25, .05, .00,},
                new[] {.10, .10, .10, .15, .25, .25, .05, .00,},
                new[] {.10, .10, .10, .10, .20, .25, .10, .05,},
                new[] {.10, .10, .10, .10, .20, .25, .10, .05,},
                new[] {.10, .10, .10, .10, .15, .20, .15, .10,},
                new[] {.10, .10, .10, .10, .15, .20, .15, .10,},
                new[] {.10, .10, .10, .10, .15, .20, .15, .10,}
            };
        }

        public virtual ushort[] StrongholdUnitType()
        {
            return new ushort[] {101, 102, 105, 103, 104, 107, 106, 108};
        }

        public virtual decimal StrongholdVictoryPoint(IStronghold stronghold)
        {
            if (stronghold.StrongholdState != StrongholdState.Occupied)
            {
                return 0;
            }

            var serverUptime = (decimal)SystemClock.Now.Subtract((DateTime)SystemVariableManager["Server.date"].Value).TotalDays;
            var daysOccupied = (decimal)SystemClock.Now.Subtract(stronghold.DateOccupied).TotalDays;
            return ((daysOccupied + stronghold.BonusDays) / 2m + serverUptime / 5 + 10) * (5 + stronghold.Lvl * 5m);
        }
    }
}
