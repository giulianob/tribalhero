using System.Linq;
using Game.Data.Forest;
using Game.Logic.Formulas;
using Game.Setup;

namespace Game.Map.LocationStrategies
{
    public class CityTileNextAvailableLocationStrategy : ILocationStrategy
    {
        private const int SKIP = 1;
        
        private readonly MapFactory mapFactory;

        private readonly Formula formula;

        private readonly IForestManager forestManager;

        private readonly IGameObjectLocator gameObjectLocator;

        public CityTileNextAvailableLocationStrategy(MapFactory mapFactory,
                                                     Formula formula,
                                                     IForestManager forestManager,
                                                     IGameObjectLocator gameObjectLocator)
        {
            this.mapFactory = mapFactory;
            this.formula = formula;
            this.forestManager = forestManager;
            this.gameObjectLocator = gameObjectLocator;
        }

        public Error NextLocation(out Position position)
        {
            position = new Position();
            var locations = mapFactory.Locations;
            do
            {
                if (mapFactory.Index >= locations.Count)
                {
                    return Error.MapFull;
                }

                Position point = locations[mapFactory.Index];

                mapFactory.Index += SKIP;

                // Check if objects already on that point
                var objects = gameObjectLocator.Regions.GetObjectsInTile(point.X, point.Y);

                if (objects == null)
                {
                    continue;
                }

                if (objects.Any())
                {
                    continue;
                }

                if (forestManager.HasForestNear(point.X, point.Y, formula.GetInitialCityRadius()))
                {
                    continue;
                }

                position.X = point.X;
                position.Y = point.Y;
                if(mapFactory.Index % 10 == 0)
                {
                    mapFactory.Index += 1;
                }
                break;
            }
            while (true);

            return Error.Ok;
        
        }
    }
}
