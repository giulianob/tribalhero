using System;
using System.Linq;
using Game.Map;
using Game.Setup;

namespace Game.Data.BarbarianTribe
{
    class BarbarianTribeConfigurator : IBarbarianTribeConfigurator
    {
        private static readonly int[] LevelProbability = {0, 8, 23, 37, 49, 60, 70, 78, 85, 93, 100};

        private readonly MapFactory mapFactory;

        private readonly ITileLocator tileLocator;

        private readonly IRegionManager regionManager;

        private const int MinDistanceAwayFromCities = 8;

        private readonly Random random = new Random();
        
        public BarbarianTribeConfigurator(MapFactory mapFactory, ITileLocator tileLocator, IRegionManager regionManager)
        {
            this.mapFactory = mapFactory;            
            this.tileLocator = tileLocator;
            this.regionManager = regionManager;
        }

        private bool TooCloseToCities(uint x, uint y, int minDistance)
        {
            return mapFactory.Locations().Any(loc => tileLocator.TileDistance(new Position(x, y), 1, loc, 1) <= minDistance);
        }

        public bool Next(int count, out byte level, out Position position)
        {            
            var limit = 0;
            do
            {
                position = new Position((uint)Config.Random.Next(30, (int)Config.map_width - 30),
                                        (uint)Config.Random.Next(30, (int)Config.map_height - 30));

                if (limit++ > 10000)
                {
                    level = 0;
                    position = new Position();
                    return false;
                }
            }
            while (!IsLocationAvailable(position));
            
            var ratio = count / 100m;
            var index = random.Next(count);
            for (level = 1; index >= (decimal)LevelProbability[level] * ratio; level++)
            {
                // this gets the level based on distribution
            }
            
            return true;
        }

        public bool IsLocationAvailable(Position position)
        {
            return !TooCloseToCities(position.X, position.Y, MinDistanceAwayFromCities) && !regionManager.GetObjectsWithin(position.X, position.Y, 2).Any();
        }
    }
}
