using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
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

        public CityTileNextToFriendLocationStrategy(int distance, IPlayer player, MapFactory mapFactory, TileLocator tileLocator, Random random, Formula formula)
        {
            this.distance = distance;
            this.player = player;
            this.mapFactory = mapFactory;
            this.tileLocator = tileLocator;
            this.random = random;
            this.formula = formula;
        }

        #region Implementation of ILocationStrategy

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
            List<ISimpleGameObject> objects = World.Current.GetObjects(position.X, position.Y);

            if (objects.Count > 0)
            {
                return false;
            }

            if (ForestManager.HasForestNear(position.X, position.Y, formula.GetInitialCityRadius()))
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
