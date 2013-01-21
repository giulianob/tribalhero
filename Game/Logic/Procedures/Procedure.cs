using Game.Logic.Formulas;
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

        public Procedure()
        {
        }

        public Procedure(IRegionManager regions, Formula formula, IWorld world, TileLocator tileLocator, IDbManager dbPersistance, ILocker locker)
        {
            this.regions = regions;
            this.formula = formula;
            this.world = world;
            this.tileLocator = tileLocator;
            this.dbPersistance = dbPersistance;
            this.locker = locker;
        }

        public static Procedure Current { get; set; }
    }
}