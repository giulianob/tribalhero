﻿using System.Linq;
using Game.Map;
using Game.Setup;

namespace Game.Data.Stronghold
{
    class StrongholdActivationCondition : IStrongholdActivationCondition
    {
        private readonly int radiusPerLevel = Config.stronghold_radius_per_level;
        private readonly int radiusBase = Config.stronghold_radius_base;
        private readonly int cityPerLevel = Config.stronghold_radius_per_level;
        private readonly IWorld world;

        public StrongholdActivationCondition(IWorld world)
        {
            this.world = world;
        }

        #region Implementation of IStrongholdActivationCondition

        public bool ShouldActivate(IStronghold stronghold)
        {
            var structures = world.GetObjectsWithin(stronghold.X, stronghold.Y, radiusBase + stronghold.Lvl*radiusPerLevel).OfType<Structure>();
            int citiesInRegion = structures.Select(s => s.City).Distinct().Count();

            return citiesInRegion >= cityPerLevel*stronghold.Lvl;
        }

        #endregion
    }
}