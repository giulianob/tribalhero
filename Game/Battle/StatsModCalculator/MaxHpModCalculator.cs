using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Battle.StatsModCalculator
{
    public class MaxHpModCalculator
    {
        private readonly Dictionary<string, List<double>> parameters = new Dictionary<string, List<double>>();

        private readonly double baseValue;

        public MaxHpModCalculator(double baseValue)
        {
            this.baseValue = baseValue;

            parameters.Add("PERCENT_BONUS", new List<double>());
        }

        public double GetResult()
        {
            var percentBonus = parameters["PERCENT_BONUS"];

            if (percentBonus.Count == 0)
            {
                return baseValue;
            }

            return baseValue * (Math.Min(parameters["PERCENT_BONUS"].Sum(), 140) / 100);
        }

        public void AddMod(string name, int value)
        {
            parameters[name].Add(value);
        }
    }
}