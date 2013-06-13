using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Map.LocationStrategies
{
    public interface ILocationStrategyFactory
    {
        CityTileNextAvailableLocationStrategy CreateCityTileNextAvailableLocationStrategy();
        CityTileNextToFriendLocationStrategy CreateCityTileNextToFriendLocationStrategy(int distance, IPlayer player);
    }
}
