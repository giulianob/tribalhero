using System.Collections.Generic;
using System.Linq;
using Game.Data;
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

        public CityTileNextAvailableLocationStrategy(MapFactory mapFactory, Formula formula, IForestManager forestManager)
        {
            this.mapFactory = mapFactory;
            this.formula = formula;
            this.forestManager = forestManager;
        }

        public Error NextLocation(out Position position)
        {
            position = new Position(0, 0);
            var locations = mapFactory.Locations().ToList();
            do
            {
                if (mapFactory.Index >= mapFactory.Locations().Count())
                {
                    return Error.MapFull;
                }

                Position point = locations[mapFactory.Index];

                mapFactory.Index += SKIP;

                // Check if objects already on that point
                List<ISimpleGameObject> objects = World.Current.GetObjects(point.X, point.Y);

                if (objects == null)
                {
                    continue;
                }

                if (objects.Count != 0)
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
