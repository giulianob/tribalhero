using Game.Logic.Formulas;
using Game.Logic.Triggers;
using Game.Map;
using Game.Util.Locking;
using Persistance;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        private readonly IRegionManager regions;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly TileLocator tileLocator;

        private readonly IDbManager dbPersistance;

        private readonly ILocker locker;

        private readonly CityTriggerManager cityTriggerManager;
        private readonly ICityEventFactory cityEventFactory;

        public Procedure()
        {
        }

        public Procedure(IRegionManager regions, Formula formula, IWorld world, TileLocator tileLocator, IDbManager dbPersistance, ILocker locker, CityTriggerManager cityTriggerManager, ICityEventFactory cityEventFactory)
        {
            this.regions = regions;
            this.formula = formula;
            this.world = world;
            this.tileLocator = tileLocator;
            this.dbPersistance = dbPersistance;
            this.locker = locker;
            this.cityTriggerManager = cityTriggerManager;
            this.cityEventFactory = cityEventFactory;
        }

        public static Procedure Current { get; set; }
    }
}