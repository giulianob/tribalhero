using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Battle.StatsModCalculator
{
    public class IntStatsModCalculator
    {
        private readonly Dictionary<string, List<double>> parameters = new Dictionary<string, List<double>>();

        private readonly int baseValue;

        public IntStatsModCalculator(int baseValue)
        {
            this.baseValue = baseValue;
            
            parameters.Add("RAW_BONUS", new List<double> { 0 });
            parameters.Add("PERCENT_BONUS", new List<double> { 100 });
        }

        public int GetResult()
        {
            return (int)((baseValue + parameters["RAW_BONUS"].Sum()) * parameters["PERCENT_BONUS"].Aggregate((total, each) => total * each) / Math.Pow(100, parameters["PERCENT_BONUS"].Count));
        }

        public void AddMod(string name, int value)
        {
            parameters[name].Add(value);
        }
    }
}