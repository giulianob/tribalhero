#region

using Game.Data;
using Game.Data.Stats;

#endregion

namespace Game.Battle.StatsModCalculator
{
    public class BattleStatsModCalculator
    {
        private readonly IBaseBattleStats baseStats;

        public BattleStatsModCalculator(IBaseBattleStats baseStats)
        {
            MaxHp = new MaxHpModCalculator((double)baseStats.MaxHp);
            Atk = new AtkDmgModCalculator((double)baseStats.Attack);
            Splash = new IntStatsModCalculator(baseStats.Splash);
            Rng = new IntStatsModCalculator(baseStats.Range);
            Stl = new IntStatsModCalculator(baseStats.Stealth);
            Spd = new IntStatsModCalculator(baseStats.Speed);
            Carry = new IntStatsModCalculator(baseStats.Carry);
            
            this.baseStats = baseStats;
        }

        public MaxHpModCalculator MaxHp { get; private set; }

        public AtkDmgModCalculator Atk { get; private set; }

        public IntStatsModCalculator Splash { get; private set; }

        public IntStatsModCalculator Rng { get; private set; }

        public IntStatsModCalculator Stl { get; private set; }

        public IntStatsModCalculator Spd { get; private set; }

        public IntStatsModCalculator Carry { get; private set; }

        public BattleStats GetStats()
        {
            var stats = new BattleStats(baseStats)
            {
                    Atk = (decimal)Atk.GetResult(),
                    Splash = (byte)Splash.GetResult(),
                    Rng = (byte)Rng.GetResult(),
                    Spd = (byte)Spd.GetResult(),
                    Stl = (byte)Stl.GetResult(),
                    Carry = (ushort)Carry.GetResult(),
                    MaxHp = (decimal)MaxHp.GetResult(),
                    NormalizedCost = baseStats.NormalizedCost
            };

            return stats;
        }
    }
}