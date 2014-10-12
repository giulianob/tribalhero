using System;
using System.Collections.Generic;
using System.Linq;
using Game.Map;
using Game.Setup;
using Game.Util;

namespace Game.Data.BarbarianTribe
{
    // Code is originally from Jake. See barbspawn.js for original reference.
    class BarbarianTribeWeightedConfigurator : IBarbarianTribeConfigurator
    {
        private const int REGION_WIDTH = 100; // Region x-size is row_x + reg_width
        private const int REGION_HEIGHT = 200; // Region y-size is row_y + reg_height
        private const int MIN_BARBS_PER_REGION = 1; // Each region should have at least this many barbs
        private const int MAX_BARBS_PER_REGION = 9; // Each region can have this many barbs
        private const int CITY_IP_PER_BARB_LVL = 35; 

        private readonly MapFactory mapFactory;

        private readonly IRegionManager regionManager;

        private readonly ICityManager cityManager;

        public BarbarianTribeWeightedConfigurator(MapFactory mapFactory,
                                                  IRegionManager regionManager,
                                                  ICityManager cityManager)
        {
            this.mapFactory = mapFactory;
            this.regionManager = regionManager;
            this.cityManager = cityManager;

            if (Config.map_width % REGION_WIDTH != 0)
            {
                throw new Exception("Map width must be divisible by barbarian tribe width");
            }

            if (Config.map_height % REGION_WIDTH != 0)
            {
                throw new Exception("Map height must be divisible by barbarian tribe width");
            }
        }

        public IEnumerable<BarbarianTribeConfiguration> Create(IEnumerable<IBarbarianTribe> existingBarbarianTribes, int existingBarbarianTribeCount)
        {
            var existingBarbarianTribesList = existingBarbarianTribes.ToList();

            for (uint rowY = 0; rowY < Config.map_height; rowY += REGION_HEIGHT)
            {
                for (uint rowX = 0; rowX < Config.map_width; rowX += REGION_WIDTH)
                {
                    var region = GenerateRegion(new Position(rowX, rowY), existingBarbarianTribesList);
                    var barbarianTribesToCreateCount = GetBarbarianTribesToGenerate(region);

                    foreach (var barbarianTribe in AddBarbarianTribesToRegion(region, barbarianTribesToCreateCount))
                    {
                        yield return barbarianTribe;
                    }
                }
            }
        }

        private IEnumerable<BarbarianTribeConfiguration> AddBarbarianTribesToRegion(ConfigurationRegion region, int barbarianTribesToCreate)
        {
            var maxTries = barbarianTribesToCreate * 25; // The maximum number of valid point generation attempts
            var i = 0;

            var ipGroups = region.CityIps.GroupBy(cityIp => cityIp / (CITY_IP_PER_BARB_LVL*2))
                                     .Select(group => new {GroupIp = group.Key, CityIps = group.ToList()})
                                     .OrderByDescending(group => group.GroupIp)
                                     .ToArray();

            while (maxTries > 0 && i < barbarianTribesToCreate)
            {
                var position = region.GetRandomPosition();
                if (!IsLocationAvailable(position))
                {
                    maxTries--;
                    continue;
                }

                int cityIp;
                if (i < ipGroups.Length && ipGroups[i].CityIps.Count > 0)
                {
                    cityIp = ipGroups[i].CityIps.First();
                    ipGroups[i].CityIps.RemoveAt(0);
                }
                else
                {
                    cityIp = ipGroups.SelectMany(group => @group.CityIps).FirstOrDefault();
                }

                byte level;
                // Make sure non-empty regions have at least 1 low level barb
                if (region.CityIps.Count > 0 && i + 1 == barbarianTribesToCreate && region.BarbLevels.All(barbLevel => barbLevel < 2 || barbLevel > 4))
                {
                    level = (byte)Config.Random.Next(2, 5);
                }
                // If we are generating for a specific city 
                else if (cityIp > 0)
                {
                    // Returns a random barb level within a 5 level range.
                    // The range is determined by the provided city_ip. 
                    // E.g., a city with 150 ip will result in barb lvl 4-8.
                    level = (byte)((cityIp / CITY_IP_PER_BARB_LVL).Clamp(1, 6) + Config.Random.Next(0, 5));
                }
                // Pick a random one if we already have a low level and we are generating the free barb
                else
                {
                    level = (byte)Config.Random.Next(2, 11);
                }

                level = level.Clamp((byte)1, (byte)10);

                region.BarbLevels.Add(level);

                yield return new BarbarianTribeConfiguration(level, position);

                i++;
            }
        }

        private int GetBarbarianTribesToGenerate(ConfigurationRegion region)
        {
            var cityCount = region.CityIps.Count;
            var barbCount = region.BarbLevels.Count;

            var neededSpawns = MIN_BARBS_PER_REGION; // Start with min amount
            neededSpawns += cityCount; // Add another barb for each city
            neededSpawns = Math.Min(neededSpawns, MAX_BARBS_PER_REGION); // Cap result
            neededSpawns -= barbCount; // Subtract existing barbs from result
            neededSpawns = Math.Max(0, neededSpawns); // Keep result at 0 or higher

            return neededSpawns;
        }

        private ConfigurationRegion GenerateRegion(Position regionPosition, List<IBarbarianTribe> existingBarbarianTribes)
        {
            var region = new ConfigurationRegion(regionPosition);

            foreach (var barbarianTribe in existingBarbarianTribes
                .Where(barbarianTribe => region.IsWithinRegion(barbarianTribe.PrimaryPosition)))
            {
                region.BarbLevels.Add(barbarianTribe.Lvl);
            }

            // We are not locking cities here but all we are accessing is the primary position and the value
            // which are never null
            foreach (var city in cityManager.AllCities().Where(city => region.IsWithinRegion(city.PrimaryPosition)))
            {
                region.CityIps.Add(city.Value);
            }

            return region;
        }
        
        public bool IsLocationAvailable(Position position)
        {
            return !regionManager.GetObjectsWithin(position.X, position.Y, BarbarianTribe.SIZE + 1).Any() && !mapFactory.TooCloseToCities(position, BarbarianTribe.SIZE);
        }

        private class ConfigurationRegion
        {
            private Position position;

            public readonly List<int> CityIps = new List<int>(); // Array of influence point values for each city

            public readonly List<byte> BarbLevels = new List<byte>(); // Array of existing barb levels

            public ConfigurationRegion(Position position)
            {
                this.position = position;
            }

            public bool IsWithinRegion(Position positionToCheck)
            {
                return (positionToCheck.X >= position.X
                        && positionToCheck.X <= (position.X + REGION_WIDTH)
                        && positionToCheck.Y >= position.Y
                        && positionToCheck.Y <= (position.Y + REGION_HEIGHT));
            }

            public Position GetRandomPosition()
            {
                return new Position(
                        (uint)Config.Random.Next((int)position.X, (int)(position.X + REGION_WIDTH)),
                        (uint)Config.Random.Next((int)position.Y, (int)(position.Y + REGION_HEIGHT)));
            }
        }
    }
}