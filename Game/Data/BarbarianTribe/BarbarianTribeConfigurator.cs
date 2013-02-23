using System;
using System.Linq;
using Game.Map;
using Game.Setup;

namespace Game.Data.BarbarianTribe
{
    class BarbarianTribeConfigurator : IBarbarianTribeConfigurator
    {
        private static readonly int[] LevelProbability = new[] {0, 8, 23, 37, 49, 60, 70, 78, 85, 93, 100};

        private readonly MapFactory mapFactory;

        private readonly TileLocator tileLocator;

        private readonly IRegionManager regionManager;

        private const int MinDistanceAwayFromCities = 10;

        private readonly Random random = new Random();
        
        public BarbarianTribeConfigurator(MapFactory mapFactory, TileLocator tileLocator, IRegionManager regionManager)
        {
            this.mapFactory = mapFactory;            
            this.tileLocator = tileLocator;
            this.regionManager = regionManager;
        }

        private bool TooCloseToCities(uint x, uint y, int minDistance)
        {
            return mapFactory.Locations().Any(loc => tileLocator.TileDistance(x, y, loc.X, loc.Y) < minDistance);
        }

        public bool Next(int count, out byte level, out uint x, out uint y)
        {            
            var limit = 0;
            do
            {
                x = (uint)Config.Random.Next(30, (int)Config.map_width - 30);
                y = (uint)Config.Random.Next(30, (int)Config.map_height - 30);

                if (limit++ > 10000)
                {
                    level = 0;
                    x = 0;
                    y = 0;
                    return false;
                }
            }
            while (TooCloseToCities(x, y, MinDistanceAwayFromCities) || regionManager.GetObjectsWithin(x, y, 5).Count > 0);
         
            var ratio = count / 100m;
            var index = random.Next(count);
            for (level = 1; index >= (decimal)LevelProbability[level] * ratio; level++)
            {
                // this gets the level based on distribution
            }
            
            return true;
        }
    }
}
