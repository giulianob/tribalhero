using System.Collections.Generic;
using System.Linq;
using Game.Map;
using Game.Setup;

namespace Game.Data.BarbarianTribe
{
    class BarbarianTribeRandomConfigurator : IBarbarianTribeConfigurator
    {
        private static readonly int[] LevelProbability = {0, 8, 23, 37, 49, 60, 70, 78, 85, 93, 100};

        private readonly MapFactory mapFactory;

        private readonly IRegionManager regionManager;

        public BarbarianTribeRandomConfigurator(MapFactory mapFactory, IRegionManager regionManager)
        {
            this.mapFactory = mapFactory;
            this.regionManager = regionManager;
        }

        private bool Next(out byte level, out Position position)
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
            
            var ratio = Config.barbariantribe_generate / 100m;
            var index = Config.Random.Next(Config.barbariantribe_generate);

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            for (level = 1; index >= (decimal)LevelProbability[level] * ratio; level++)
            {
                // this gets the level based on distribution
            }
            
            return true;
        }

        public IEnumerable<BarbarianTribeConfiguration> Create(IEnumerable<IBarbarianTribe> existingBarbarianTribes, int existingBarbarianTribeCount)
        {
            var toGenerate = Config.barbariantribe_generate - existingBarbarianTribeCount;

            for (var i = 0; i < toGenerate; i++)
            {
                byte level;
                Position position;
                if (!Next(out level, out position))
                {
                    continue;
                }

                yield return new BarbarianTribeConfiguration(level, position);
            }
        }

        public bool IsLocationAvailable(Position position)
        {
            return !regionManager.GetObjectsWithin(position.X, position.Y, BarbarianTribe.SIZE).Any() && !mapFactory.TooCloseToCities(position, BarbarianTribe.SIZE);
        }
    }
}
