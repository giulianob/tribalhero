namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeFactory
    {
        BarbarianTribe CreateBarbarianTribe(uint id, string name, byte level, uint x, uint y, int count);
    }
}
