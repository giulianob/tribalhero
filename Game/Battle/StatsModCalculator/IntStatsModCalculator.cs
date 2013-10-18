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

            parameters.Add("RAW_BONUS", new List<double>());
            parameters.Add("PERCENT_BONUS", new List<double>());
        }

        public int GetResult()
        {
            return (int)((baseValue + parameters["RAW_BONUS"].Sum()) * (100 + parameters["PERCENT_BONUS"].Sum()) / 100);
        }

        public void AddMod(string name, int value)
        {
            parameters[name].Add(value);
        }
    }
}