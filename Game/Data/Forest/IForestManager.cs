using Game.Util.Locking;

namespace Game.Data.Forest
{
    public interface IForestManager
    {
        int[] ForestCount { get; }

        void StartForestCreator();

        void DbLoaderAdd(IForest forest);

        bool HasForestNear(uint x, uint y, int radius);

        void CreateForest(byte lvl, int capacity, double rate);

        void CreateForestAt(byte lvl, int capacity, double rate, uint x = 0, uint y = 0);

        void RemoveForest(IForest forest);

        /// <summary>
        ///     Locks all cities participating in this forest.
        ///     Once inside of the lock, a call to ForestManager.TryGetStronghold should be used to get the forest.
        /// </summary>
        /// <param name="custom">custom[0] should contain the forestId to lock</param>
        /// <returns>List of cities to lock for the forest.</returns>
        ILockable[] CallbackLockHandler(object[] custom);

        bool TryGetValue(uint id, out IForest forest);

        void RegenerateForests();
    }
}