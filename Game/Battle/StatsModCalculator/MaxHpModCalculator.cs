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
            parameters.Add("BOW_SHIELD_BONUS", new List<double> {0});
        }

        public double GetResult()
        {
            return baseValue * ((100 + Math.Min(parameters["PERCENT_BONUS"].Sum(), 40) + parameters["BOW_SHIELD_BONUS"].Max()) / 100);
        }

        public void AddMod(string name, int value)
        {
            parameters[name].Add(value);
        }
    }
}