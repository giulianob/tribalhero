using Game.Data.Troop;

namespace Game.Data
{
    public interface ISimpleStubGenerator
    {
        /// <summary>
        ///     This method generates simplestub.
        /// </summary>
        /// <param name="level">level of object</param>
        /// <param name="upkeep">Total output upkeep</param>
        /// <param name="unitLevel">The unit level to generate</param>
        /// <param name="randomness">A percentage of upkeep being completely random, 1 means it's completely random</param>
        /// <param name="seed">seed for the randomizer</param>
        /// <param name="stub">output</param>
        void Generate(int level, int upkeep, byte unitLevel, double randomness, int seed, out ISimpleStub stub);
    }
}