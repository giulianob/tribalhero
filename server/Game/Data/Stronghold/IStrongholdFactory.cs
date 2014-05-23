namespace Game.Data.Stronghold
{
    public interface IStrongholdFactory
    {
        IStronghold CreateStronghold(uint id, string name, byte level, uint x, uint y, decimal gate, int gateMax);
    }
}