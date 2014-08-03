using Game.Map;

namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeFactory
    {
        BarbarianTribe CreateBarbarianTribe(uint id, byte level, Position position, int count);
    }
}
