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

        private readonly IRegionManager regionManager;

        private readonly ITileLocator tileLocator;

        public BarbarianTribeConfigurator(MapFactory mapFactory, IRegionManager regionManager, ITileLocator tileLocator)
        {
            this.mapFactory = mapFactory;
            this.regionManager = regionManager;
            this.tileLocator = tileLocator;
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
            var index = Config.Random.Next(count);
            for (level = 1; index >= (decimal)LevelProbability[level] * ratio; level++)
            {
                // this gets the level based on distribution
            }
            
            return true;
        }

        public bool IsLocationAvailable(Position position)
        {
            return !regionManager.GetObjectsWithin(position.X, position.Y, BarbarianTribe.SIZE).Any() && !mapFactory.TooCloseToCities(position, BarbarianTribe.SIZE);
        }
    }
}
