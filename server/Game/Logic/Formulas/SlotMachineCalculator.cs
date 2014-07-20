using System;
using Game.Util;

namespace Game.Logic.Formulas
{
    public class SlotMachineCalculator
    {
        private readonly int[] minValue;

        private readonly int[] maxValue;

        private readonly int[] missValue;

        private readonly double[] chance;

        protected SlotMachineCalculator()
        {            
        }

        public SlotMachineCalculator(int[] minValue, int[] maxValue, double[] chance, int[] missValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.missValue = missValue;
            this.chance = chance;

            if (minValue.Length != maxValue.Length || maxValue.Length != chance.Length || maxValue.Length != missValue.Length)
            {
                throw new ArgumentException("Arrays are not all the same size");
            }
        }

        public virtual int Roll(byte level, Random rand)
        {
            lock (rand)
            {
                if (level == 0 || level > minValue.Length)
                {
                    throw new ArgumentOutOfRangeException("level");
                }

                if (rand.NextDouble() >= chance[level - 1])
                {
                    // Missed
                    return missValue[level - 1];
                }

                return rand.Next(minValue[level - 1], maxValue[level - 1] + 1);
            }
        }
    }
}
