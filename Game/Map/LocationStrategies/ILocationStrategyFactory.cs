using Game.Data;

namespace Game.Map.LocationStrategies
{
    public interface ILocationStrategyFactory
    {
        CityTileNextAvailableLocationStrategy CreateCityTileNextAvailableLocationStrategy();

        CityTileNextToFriendLocationStrategy CreateCityTileNextToFriendLocationStrategy(int distance, IPlayer player);
    }
}
