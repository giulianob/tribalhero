using Game.Setup;

namespace Game.Map.LocationStrategies
{
    public interface ILocationStrategy
    {
        Error NextLocation(out Position position);
    }
}
