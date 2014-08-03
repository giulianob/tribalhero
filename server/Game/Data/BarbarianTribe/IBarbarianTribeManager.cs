namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeManager
    {
        int Count { get; }

        void DbLoaderAdd(IBarbarianTribe barbarianTribe);

        bool CreateBarbarianTribeNear(byte level, int campCount, uint x, uint y, byte radius);

        bool TryGetBarbarianTribe(uint id, out IBarbarianTribe barbarianTribe);

        void Generate(int count);
        
        void RelocateAsNeeded();

        bool RelocateBarbarianTribe(IBarbarianTribe barbarianTribe);
    }
}
