using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Map;
using Game.Setup;
using Game.Util;

namespace Game.Data.Stronghold
{
    class StrongholdConfigurator : IStrongholdConfigurator
    {
        private const int MIN_DISTANCE_AWAY_FROM_CITIES = 10;

        private const int MIN_DISTANCE_AWAY_FROM_STRONGHOLDS = 200;

        private static readonly int citiesPerLevel = Config.stronghold_cities_per_level;

        private static readonly int radiusPerLevel = Config.stronghold_radius_per_level;

        private static readonly int radiusBase = Config.stronghold_radius_base;

        private readonly MapFactory mapFactory;

        private readonly NameGenerator nameGenerator =
                new NameGenerator(Path.Combine(Config.maps_folder, "strongholdnames.txt"));

        private readonly List<Position> strongholds = new List<Position>();

        private readonly TileLocator tileLocator;

        public StrongholdConfigurator(MapFactory mapFactory, TileLocator tileLocator)
        {
            this.mapFactory = mapFactory;
            this.tileLocator = tileLocator;
        }

        private bool HasEnoughCities(uint x, uint y, int level)
        {
            int radius = radiusBase + (level - 1) * radiusPerLevel;
                    // basic radius 200, then every each level is 50 radius
            int count = level * citiesPerLevel;

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

        public bool Next(out string name, out byte level, out uint x, out uint y)
        {
            if (!nameGenerator.Next(out name))
            {
                name = "";
                level = 0;
                x = 0;
                y = 0;
                return false;
            }
            level = (byte)(Config.Random.Next(20) + 1);
            do
            {
                x = (uint)Config.Random.Next(30, (int)Config.map_width - 30);
                y = (uint)Config.Random.Next(30, (int)Config.map_height - 30);
            }
            while (!HasEnoughCities(x, y, level) || TooCloseToCities(x, y, MIN_DISTANCE_AWAY_FROM_CITIES) ||
                   TooCloseToStrongholds(x, y, MIN_DISTANCE_AWAY_FROM_STRONGHOLDS));

            strongholds.Add(new Position(x, y));
            Global.Logger.Info(string.Format("Added stronghold[{0},{1}] Number[{2}] ", x, y, strongholds.Count));
            return true;
        }

        #endregion
    }
}