using Game.Map;

namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeConfigurator
    {
        bool Next(int count, out byte level, out Position position);

        bool IsLocationAvailable(Position position);
    }
}
