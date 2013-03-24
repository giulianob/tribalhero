using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Map;
using Game.Setup;
using Game.Util;
using Ninject.Extensions.Logging;

namespace Game.Data.Stronghold
{
    class StrongholdConfigurator : IStrongholdConfigurator
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private static readonly int[] LevelProbability = new[] {0, 8, 16, 23, 30, 37, 43, 49, 55, 60, 65, 70, 74, 78, 82, 85, 88, 91, 94, 97, 100};

        private const int MinDistanceAwayFromCities = 10;

        private const int MinDistanceAwayFromStrongholds = 200;

        private static readonly int CitiesPerLevel = Config.stronghold_cities_per_level;

        private static readonly int RadiusPerLevel = Config.stronghold_radius_per_level;

        private static readonly int RadiusBase = Config.stronghold_radius_base;

        private readonly MapFactory mapFactory;

        private readonly NameGenerator nameGenerator;

        private readonly List<Position> strongholds = new List<Position>();

        private readonly TileLocator tileLocator;

        private readonly IRegionManager regionManager;

        public StrongholdConfigurator(NameGenerator nameGenerator, MapFactory mapFactory, TileLocator tileLocator, IRegionManager regionManager)
        {
            this.nameGenerator = nameGenerator;
            this.mapFactory = mapFactory;
            this.tileLocator = tileLocator;
            this.regionManager = regionManager;
        }

        private bool HasEnoughCities(uint x, uint y, int level)
        {
            int radius = RadiusBase + (level - 1) * RadiusPerLevel;
            // basic radius 200, then every each level is 50 radius
            int count = level * CitiesPerLevel;

            return mapFactory.Locations().Count(loc => tileLocator.TileDistance(x, y, loc.X, loc.Y) < radius) > count;
        }

        private bool TooCloseToCities(uint x, uint y, int minDistance)
        {
            return mapFactory.Locations().Any(loc => tileLocator.TileDistance(x, y, loc.X, loc.Y) < minDistance);
        }

        private bool TooCloseToStrongholds(uint x, uint y, int minDistance)
        {
            return strongholds.Any(loc => tileLocator.TileDistance(x, y, loc.X, loc.Y) < minDistance);
        }

        #region Implementation of IStrongholdConfigurator

        public bool Next(int index, int count, out string name, out byte level, out uint x, out uint y)
        {
            if (!nameGenerator.Next(out name))
            {
                name = "";
                level = 0;
                x = 0;
                y = 0;
                return false;
            }

            var ratio = count / 100m;
            for (level = 1; index >= (decimal)LevelProbability[level] * ratio; level++)
            {
                // this gets the level based on distribution
            }

            var limit = 0;
            do
            {
                x = (uint)Config.Random.Next(30, (int)Config.map_width - 30);
                y = (uint)Config.Random.Next(30, (int)Config.map_height - 30);
                                
                if (limit++ > 10000)
                {
                    name = "";
                    level = 0;
                    x = 0;
                    y = 0;
                    return false;
                }
            }
            while (!HasEnoughCities(x, y, level) || TooCloseToCities(x, y, MinDistanceAwayFromCities) ||
                   TooCloseToStrongholds(x, y, MinDistanceAwayFromStrongholds) || regionManager.GetObjects(x, y).Count > 0);

            strongholds.Add(new Position(x, y));
            
            logger.Info(string.Format("Added stronghold[{0},{1}] Number[{2}] ", x, y, strongholds.Count));
            
            return true;
        }

        #endregion
    }
}