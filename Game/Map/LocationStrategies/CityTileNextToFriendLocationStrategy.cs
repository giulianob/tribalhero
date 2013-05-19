using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Forest;
using Game.Logic.Formulas;
using Game.Setup;

namespace Game.Map.LocationStrategies
{
    public class CityTileNextToFriendLocationStrategy : ILocationStrategy
    {
        private readonly int distance;
        
        private readonly IPlayer player;
        
        private readonly MapFactory mapFactory;
        
        private readonly TileLocator tileLocator;
        
        private readonly Random random;

        private readonly Formula formula;

        private readonly IForestManager forestManager;

        private readonly IWorld world;

        public CityTileNextToFriendLocationStrategy(int distance,
                                                    IPlayer player,
                                                    MapFactory mapFactory,
                                                    TileLocator tileLocator,
                                                    Random random,
                                                    Formula formula,
                                                    IForestManager forestManager,
                                                    IWorld world)
        {
            this.distance = distance;
            this.player = player;
            this.mapFactory = mapFactory;
            this.tileLocator = tileLocator;
            this.random = random;
            this.formula = formula;
            this.forestManager = forestManager;
            this.world = world;
        }

        public Error NextLocation(out Position position)
        {
            var city = player.GetCityList().FirstOrDefault();

            if (city == null)
            {
                position = null;
                return Error.CityNotFound;
            }

            var positions = mapFactory.Locations().Where(loc =>Predicate(loc,city)).ToList();

            if (positions.Count == 0)
            {
                position = null;
                return Error.FriendMapFull;
            }

            position = positions[random.Next(positions.Count)];
            return Error.Ok;
        }

        private bool Predicate(Position position, ICity city)
        {

            if (tileLocator.TileDistance(city.X, city.Y, position.X, position.Y) > distance)
            {
                return false;
            }

            // Check if objects already on that point
            List<ISimpleGameObject> objects = world.GetObjects(position.X, position.Y);

            return objects.Count == 0 && !forestManager.HasForestNear(position.X, position.Y, formula.GetInitialCityRadius());
        }
    }
}
