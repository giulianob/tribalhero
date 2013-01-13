using System;
using Game.Data.Troop;
using Game.Setup;

namespace Game.Data.Stronghold
{
    public class SimpleStubGenerator
    {
        private readonly double[,] ratio = new[,]
        {
                {.00, .00, .00, .00, .00, .00, .00, .00,}, {1.00, .00, .00, .00, .00, .00, .00, .00,},
                {.90, .10, .00, .00, .00, .00, .00, .00,}, {.50, .50, .00, .00, .00, .00, .00, .00,},
                {.50, .25, .25, .00, .00, .00, .00, .00,}, {.50, .25, .25, .00, .00, .00, .00, .00,},
                {.40, .25, .25, .10, .00, .00, .00, .00,}, {.40, .25, .25, .10, .00, .00, .00, .00,},
                {.35, .20, .25, .15, .05, .00, .00, .00,}, {.25, .20, .25, .15, .15, .00, .00, .00,},
                {.25, .20, .25, .15, .15, .00, .00, .00,}, {.20, .15, .20, .15, .15, .15, .00, .00,},
                {.20, .15, .20, .15, .15, .15, .00, .00,}, {.15, .10, .15, .15, .25, .20, .00, .00,},
                {.10, .10, .10, .15, .25, .25, .05, .00,}, {.10, .10, .10, .15, .25, .25, .05, .00,},
                {.10, .10, .10, .10, .20, .25, .10, .05,}, {.10, .10, .10, .10, .20, .25, .10, .05,},
                {.10, .10, .10, .10, .15, .20, .15, .10,}, {.10, .10, .10, .10, .15, .20, .15, .10,},
                {.10, .10, .10, .10, .15, .20, .15, .10,},
        };

        private readonly ushort[] type = new ushort[] {101, 102, 105, 103, 104, 107, 106, 108};

        private readonly UnitFactory unitFactory;

        public SimpleStubGenerator(UnitFactory unitFactory)
        {
            this.unitFactory = unitFactory;
        }

        /// <summary>
        ///     This method generates simplestub.
        /// </summary>
        /// <param name="level">level of stronghold</param>
        /// <param name="upkeep">Total output upkeep</param>
        /// <param name="randomness">A percentage of upkeep being completely random, 1 means it's completely random</param>
        /// <param name="seed">seed for the randomizer</param>
        /// <param name="stub">output</param>
        public virtual void Generate(int level, int upkeep, double randomness, int seed, out ISimpleStub stub)
        {
            stub = new SimpleStub();
            for (int i = 0; i < type.Length; ++i)
            {
                stub.AddUnit(FormationType.Normal,
                             type[i],
                             (ushort)(upkeep * (1 - randomness) * ratio[level, i] / unitFactory.GetUnitStats(type[i], 1).Upkeep));
            }

            Random random = new Random(seed);
            int remaining = (int)(upkeep * randomness);
            while (remaining > 0)
            {
                var nextUpkeep = random.Next(1, remaining);
                var nextType = random.Next(0, type.Length);
                var unitUpkeep = unitFactory.GetUnitStats(type[nextType], 1).Upkeep;
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