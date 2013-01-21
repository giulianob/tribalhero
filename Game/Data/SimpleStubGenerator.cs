using System;
using Game.Data.Troop;
using Game.Setup;

namespace Game.Data
{
    public class SimpleStubGenerator : ISimpleStubGenerator
    {
        private readonly UnitFactory unitFactory;

        private readonly double[,] ratio;

        private readonly ushort[] type;

        public SimpleStubGenerator(double[,] ratio, ushort[] type, UnitFactory unitFactory)
        {
            this.unitFactory = unitFactory;
            this.ratio = ratio;
            this.type = type;
        }

        /// <summary>
        ///     This method generates simplestub.
        /// </summary>
        /// <param name="level">level of object</param>
        /// <param name="upkeep">Total output upkeep</param>
        /// <param name="unitLevel">The unit level to generate</param>
        /// <param name="randomness">A percentage of upkeep being completely random, 1 means it's completely random</param>
        /// <param name="seed">seed for the randomizer</param>
        /// <param name="stub">output</param>
        public virtual void Generate(int level, int upkeep, byte unitLevel, double randomness, int seed, out ISimpleStub stub)
        {
            stub = new SimpleStub();
            for (int i = 0; i < type.Length; ++i)
            {
                stub.AddUnit(FormationType.Normal,
                             type[i],
                             (ushort)(upkeep * (1 - randomness) * ratio[level, i] / unitFactory.GetUnitStats(type[i], unitLevel).Upkeep));
            }

            Random random = new Random(seed);
            int remaining = (int)(upkeep * randomness);
            while (remaining > 0)
            {
                var nextUpkeep = random.Next(1, remaining);
                var nextType = random.Next(0, type.Length);
                var unitUpkeep = unitFactory.GetUnitStats(type[nextType], unitLevel).Upkeep;
                var unitCount = nextUpkeep / unitUpkeep;
                if (unitCount <= 0)
                {
                    continue;
                }
                stub.AddUnit(FormationType.Normal, type[nextType], (ushort)unitCount);
                remaining -= unitUpkeep * unitCount;
            }
        }
    }
}