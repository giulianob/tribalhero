using System.Collections.Generic;

namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeManager
    {
        int Count { get; }

        IEnumerable<IBarbarianTribe> AllTribes { get; }

        void DbLoaderAdd(IBarbarianTribe barbarianTribe);

        bool CreateBarbarianTribeNear(byte level, int campCount, uint x, uint y, byte radius);

        bool TryGetBarbarianTribe(uint id, out IBarbarianTribe barbarianTribe);

        void Generate();
        
        void RelocateAsNeeded();

        bool RelocateBarbarianTribe(IBarbarianTribe barbarianTribe);

        void RelocateIdleBarbCamps();
    }
}
