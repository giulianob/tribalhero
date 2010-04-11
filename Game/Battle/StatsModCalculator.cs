using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Stats;

namespace Game.Battle {
    public class ModParameter {
        List<int> list = new List<int>();

        public ModParameter(int defaultValue) {
            list.Add(defaultValue);
        }

        public void AddValue(int value) {
            list.Add(value);
        }

        public int Max { 
            get { return list.Max(); }
        }

        public int Avg {
            get { return (int)list.Average(); }
        }

        public int Min {
            get { return list.Min(); }
        }
        
        public int Sum {
            get { return list.Sum(); }
        }
        
        public int Product {
            get { return list.AsQueryable<int>().Aggregate((total, value) => total * value); }
        }

        public int Count {
            get { return list.Count; }
        }
    }
    public abstract class ModCalculator<T> {
        protected Dictionary<string, ModParameter> parameters = new Dictionary<string, ModParameter>();

        public ModCalculator() {
            SetParameters();
        }
        public void AddMod(String parameter, int value) {
            parameters[parameter].AddValue(value);
        }
        public abstract T GetResult();
        protected abstract void SetParameters();
    }

    public class IntStatsModCalculator : ModCalculator<int> {
        int baseValue;
        public IntStatsModCalculator(int baseValue) {
            this.baseValue = baseValue;
        }

        public override int GetResult() {
            return (int)((baseValue + parameters["RAW_BONUS"].Sum) * parameters["PERCENT_BONUS"].Product / Math.Pow(100, parameters["PERCENT_BONUS"].Count));
        }

        protected override void SetParameters() {
            parameters.Add("RAW_BONUS",new ModParameter(0));
            parameters.Add("PERCENT_BONUS", new ModParameter(100));
        }
    }

    public class  BattleStatsModCalculator {
        BaseBattleStats baseStats;
        public IntStatsModCalculator MaxHp;
        public IntStatsModCalculator Atk;
        public IntStatsModCalculator Def;
        public IntStatsModCalculator Rng;
        public IntStatsModCalculator Stl;
        public IntStatsModCalculator Spd;

        public BattleStatsModCalculator(BaseBattleStats baseStats) {
            MaxHp = new IntStatsModCalculator(baseStats.MaxHp);
            Atk = new IntStatsModCalculator(baseStats.Atk);
            Def = new IntStatsModCalculator(baseStats.Def);
            Rng = new IntStatsModCalculator(baseStats.Rng);
            Stl = new IntStatsModCalculator(baseStats.Stl);
            Spd = new IntStatsModCalculator(baseStats.Spd);
            this.baseStats = baseStats;
        }

        public void AddAtkParameter(string paramter, int value) {
            Atk.AddMod(paramter, value);
        }

        public void AddRngParameter(string paramter, int value) {
            Rng.AddMod(paramter, value);
        }
        public void AddStlParameter(string paramter, int value) {
            Stl.AddMod(paramter, value);
        }
        public void AddSpdParameter(string paramter, int value) {
            Spd.AddMod(paramter, value);
        }
        public void AddMaxHpParameter(string paramter, int value) {
            MaxHp.AddMod(paramter, value);
        }

        public BattleStats GetStats() {
            BattleStats stats= new BattleStats(baseStats);
            stats.Atk = (ushort)Atk.GetResult();
            stats.Def = (ushort)Def.GetResult();
            stats.Rng = (byte)Rng.GetResult();
            stats.Spd = (byte)Spd.GetResult();
            stats.Stl = (byte)Stl.GetResult();
            stats.MaxHp = (ushort)MaxHp.GetResult();
            return stats;
        }
    }
   
}
