using System.Linq;
using Game.Map;
using Game.Setup;

namespace Game.Data.Stronghold
{
    class StrongholdActivationCondition : IStrongholdActivationCondition
    {
        private readonly int cityPerLevel = Config.stronghold_cities_per_level;

        private readonly int radiusBase = Config.stronghold_radius_base;

        private readonly int radiusPerLevel = Config.stronghold_radius_per_level;

        private readonly IWorld world;

        public StrongholdActivationCondition(IWorld world)
        {
            this.world = world;
        }

        public bool ShouldActivate(IStronghold stronghold)
        {
            var citiesInRegion =
                    world.Regions.GetObjectsFromSurroundingRegions(stronghold.X,
                                                                   stronghold.Y,
                                                                   radiusBase + stronghold.Lvl * radiusPerLevel)
                         .OfType<Structure>()
                         .Select(s => s.City)
                         .Distinct()
                         .Count();

            return citiesInRegion >= cityPerLevel * stronghold.Lvl;
        }
    }
}