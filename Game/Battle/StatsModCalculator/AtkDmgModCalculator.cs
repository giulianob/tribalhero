using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Battle.StatsModCalculator
{
    public class AtkDmgModCalculator
    {
        private readonly double baseValue;

        private readonly Dictionary<string, List<double>> parameters = new Dictionary<string, List<double>>();

        public AtkDmgModCalculator(double baseValue)
        {
            this.baseValue = baseValue;

            parameters.Add("RAW_BONUS", new List<double> { 0 });
            parameters.Add("PERCENT_BONUS", new List<double> { 0 });
            parameters.Add("CALL_TO_ARM_BONUS", new List<double> { 100 });
        }

        public void AddMod(string name, int value)
        {
            parameters[name].Add(value);
        }

        public double GetResult()
        {
            return (baseValue + parameters["RAW_BONUS"].Sum()) * parameters["CALL_TO_ARM_BONUS"].Max() / 100 *
                   (100 + Math.Min(parameters["PERCENT_BONUS"].Sum(), 150)) / 100;
        }
    }
}