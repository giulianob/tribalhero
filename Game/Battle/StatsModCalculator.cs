#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stats;

#endregion

namespace Game.Battle
{
    public class ModParameter
    {
        private readonly List<double> list = new List<double>();

        public ModParameter(double defaultValue)
        {
            list.Add(defaultValue);
        }

        public double Max
        {
            get
            {
                return list.Max();
            }
        }

        public double Avg
        {
            get
            {
                return list.Average();
            }
        }

        public double Min
        {
            get
            {
                return list.Min();
            }
        }

        public double Sum
        {
            get
            {
                return list.Sum();
            }
        }

        public double Product
        {
            get
            {
                return list.AsQueryable().Aggregate((total, value) => total*value);
            }
        }

        public double Count
        {
            get
            {
                return list.Count;
            }
        }

        public void AddValue(double value)
        {
            list.Add(value);
        }
    }

    public abstract class ModCalculator<T>
    {
        protected Dictionary<string, ModParameter> parameters = new Dictionary<string, ModParameter>();

        protected ModCalculator()
        {
            SetParameters();
        }

        public void AddMod(string parameter, int value)
        {
            parameters[parameter].AddValue(value);
        }

        public abstract T GetResult();

        protected abstract void SetParameters();
    }

    public class IntStatsModCalculator : ModCalculator<int>
    {
        private readonly int baseValue;

        public IntStatsModCalculator(int baseValue)
        {
            this.baseValue = baseValue;
        }

        public override int GetResult()
        {
            return (int)((baseValue + parameters["RAW_BONUS"].Sum)*parameters["PERCENT_BONUS"].Product/Math.Pow(100, parameters["PERCENT_BONUS"].Count));
        }

        protected override void SetParameters()
        {
            parameters.Add("RAW_BONUS", new ModParameter(0));
            parameters.Add("PERCENT_BONUS", new ModParameter(100));
        }
    }

    public class BattleStatsModCalculator
    {
        private readonly BaseBattleStats baseStats;

        public BattleStatsModCalculator(BaseBattleStats baseStats)
        {
            MaxHp = new IntStatsModCalculator(baseStats.MaxHp);
            Atk = new IntStatsModCalculator(baseStats.Atk);
            Splash = new IntStatsModCalculator(baseStats.Splash);
            Def = new IntStatsModCalculator(baseStats.Def);
            Rng = new IntStatsModCalculator(baseStats.Rng);
            Stl = new IntStatsModCalculator(baseStats.Stl);
            Spd = new IntStatsModCalculator(baseStats.Spd);
            Carry = new IntStatsModCalculator(baseStats.Carry);
            this.baseStats = baseStats;
        }

        public IntStatsModCalculator MaxHp { get; private set; }
        public IntStatsModCalculator Atk { get; private set; }
        public IntStatsModCalculator Splash { get; private set; }
        public IntStatsModCalculator Def { get; private set; }
        public IntStatsModCalculator Rng { get; private set; }
        public IntStatsModCalculator Stl { get; private set; }
        public IntStatsModCalculator Spd { get; private set; }
        public IntStatsModCalculator Carry { get; private set; }

        public void AddAtkParameter(string paramter, int value)
        {
            Atk.AddMod(paramter, value);
        }

        public void AddDefParameter(string paramter, int value)
        {
            Def.AddMod(paramter, value);
        }

        public void AddSplashParameter(string paramter, int value)
        {
            Splash.AddMod(paramter, value);
        }

        public void AddRngParameter(string paramter, int value)
        {
            Rng.AddMod(paramter, value);
        }

        public void AddStlParameter(string paramter, int value)
        {
            Stl.AddMod(paramter, value);
        }

        public void AddSpdParameter(string paramter, int value)
        {
            Spd.AddMod(paramter, value);
        }

        public void AddCarryParameter(string paramter, int value) {
            Carry.AddMod(paramter, value);
        }

        public void AddMaxHpParameter(string paramter, int value)
        {
            MaxHp.AddMod(paramter, value);
        }

        public BattleStats GetStats()
        {
            var stats = new BattleStats(baseStats)
                        {
                                Atk = (ushort)Atk.GetResult(),
                                Splash = (byte)Splash.GetResult(),
                                Def = (ushort)Def.GetResult(),
                                Rng = (byte)Rng.GetResult(),
                                Spd = (byte)Spd.GetResult(),
                                Stl = (byte)Stl.GetResult(),
                                Carry = (ushort)Carry.GetResult(),
                                MaxHp = (ushort)MaxHp.GetResult()
                        };
            return stats;
        }
    }
}