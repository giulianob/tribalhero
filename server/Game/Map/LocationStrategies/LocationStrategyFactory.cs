using System;
using Game.Data;
using Game.Data.Forest;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Setup.DependencyInjection;

namespace Game.Map.LocationStrategies
{
    public class LocationStrategyFactory : ILocationStrategyFactory
    {
        private readonly IKernel kernel;

        public LocationStrategyFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public CityTileNextAvailableLocationStrategy CreateCityTileNextAvailableLocationStrategy()
        {
            return new CityTileNextAvailableLocationStrategy(kernel.Get<MapFactory>(), kernel.Get<Formula>(), kernel.Get<IForestManager>(), kernel.Get<IGameObjectLocator>());
        }

        public CityTileNextToFriendLocationStrategy CreateCityTileNextToFriendLocationStrategy(int distance, IPlayer player)
        {
            return new CityTileNextToFriendLocationStrategy(distance, player, kernel.Get<MapFactory>(), kernel.Get<ITileLocator>(), kernel.Get<Random>(), kernel.Get<Formula>(), kernel.Get<IForestManager>(), kernel.Get<IWorld>());
        }
    }
}