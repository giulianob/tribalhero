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
                return list.AsQueryable().Aggregate((total, value) => total * value);
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
            return
                    (int)
                    ((baseValue + parameters["RAW_BONUS"].Sum) * parameters["PERCENT_BONUS"].Product /
                     Math.Pow(100, parameters["PERCENT_BONUS"].Count));
        }

        protected override void SetParameters()
        {
            parameters.Add("RAW_BONUS", new ModParameter(0));
            parameters.Add("PERCENT_BONUS", new ModParameter(100));
        }
    }

    public class DoubleStatsModCalculator : ModCalculator<double>
    {
        private readonly double baseValue;

        public DoubleStatsModCalculator(double baseValue)
        {
            this.baseValue = baseValue;
        }

        public override double GetResult()
        {
            return (baseValue + parameters["RAW_BONUS"].Sum) * parameters["PERCENT_BONUS"].Product /
                   Math.Pow(100, parameters["PERCENT_BONUS"].Count);
        }

        protected override void SetParameters()
        {
            parameters.Add("RAW_BONUS", new ModParameter(0));
            parameters.Add("PERCENT_BONUS", new ModParameter(100));
        }
    }

    public class AtkDmgModCalculator : ModCalculator<double>
    {
        private readonly double baseValue;

        public AtkDmgModCalculator(double baseValue)
        {
            this.baseValue = baseValue;
        }

        public override double GetResult()
        {
            return (baseValue + parameters["RAW_BONUS"].Sum) * parameters["CALL_TO_ARM_BONUS"].Max / 100 *
                   (100 + Math.Min(parameters["PERCENT_BONUS"].Sum, 150)) / 100;
        }

        protected override void SetParameters()
        {
            parameters.Add("RAW_BONUS", new ModParameter(0));
            parameters.Add("PERCENT_BONUS", new ModParameter(0));
            parameters.Add("CALL_TO_ARM_BONUS", new ModParameter(100));
        }
    }

    public class BattleStatsModCalculator
    {
        private readonly BaseBattleStats baseStats;

        public BattleStatsModCalculator(BaseBattleStats baseStats)
        {
            MaxHp = new DoubleStatsModCalculator((double)baseStats.MaxHp);
            Atk = new AtkDmgModCalculator((double)baseStats.Atk);
            Splash = new IntStatsModCalculator(baseStats.Splash);
            Rng = new IntStatsModCalculator(baseStats.Rng);
            Stl = new IntStatsModCalculator(baseStats.Stl);
            Spd = new IntStatsModCalculator(baseStats.Spd);
            Carry = new IntStatsModCalculator(baseStats.Carry);
            NormalizedCost = new DoubleStatsModCalculator((double)baseStats.NormalizedCost);
            this.baseStats = baseStats;
        }

        public DoubleStatsModCalculator MaxHp { get; private set; }

        public AtkDmgModCalculator Atk { get; private set; }

        public IntStatsModCalculator Splash { get; private set; }

        public IntStatsModCalculator Rng { get; private set; }

        public IntStatsModCalculator Stl { get; private set; }

        public IntStatsModCalculator Spd { get; private set; }

        public IntStatsModCalculator Carry { get; private set; }

        public DoubleStatsModCalculator NormalizedCost { get; private set; }

        public void AddAtkParameter(string paramter, int value)
        {
            Atk.AddMod(paramter, value);
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

        public void AddCarryParameter(string paramter, int value)
        {
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
                    Atk = (decimal)Atk.GetResult(),
                    Splash = (byte)Splash.GetResult(),
                    Rng = (byte)Rng.GetResult(),
                    Spd = (byte)Spd.GetResult(),
                    Stl = (byte)Stl.GetResult(),
                    Carry = (ushort)Carry.GetResult(),
                    MaxHp = (decimal)MaxHp.GetResult(),
                    NormalizedCost = (decimal)NormalizedCost.GetResult(),
            };
            return stats;
        }
    }
}