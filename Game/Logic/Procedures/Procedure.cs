using Game.Data.Tribe;
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

        private readonly IDbManager dbPersistance;

        private readonly ILocker locker;

        private readonly ITribeFactory tribeFactory;

        private readonly ITribeManager tribeManager;

        public Procedure()
        {
        }

        public Procedure(IRegionManager regions, Formula formula, IWorld world, IDbManager dbPersistance, ILocker locker, ITribeFactory tribeFactory, ITribeManager tribeManager)
        {
            this.regions = regions;
            this.formula = formula;
            this.world = world;
            this.dbPersistance = dbPersistance;
            this.locker = locker;
            this.tribeFactory = tribeFactory;
            this.tribeManager = tribeManager;
        }

        public static Procedure Current { get; set; }
    }
}