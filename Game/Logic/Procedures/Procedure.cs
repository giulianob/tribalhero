using System;
using Game.Data.Tribe;
using Game.Logic.Formulas;
using Game.Map;
using Game.Util.Locking;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        private readonly IRegionManager regions;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly ILocker locker;
        
        public Procedure()
        {
        }

        // Do not add more parameters to this class. If it needs anything pass it in with the method that needs the dependency.
        // We should try to move some of the logic outside of this class since it's getting big and becoming a bag for random methods.
        // Maybe split it up into more specific types.
        public Procedure(IRegionManager regions, Formula formula, IWorld world, ILocker locker)
        {
            this.regions = regions;
            this.formula = formula;
            this.world = world;
            this.locker = locker;
        }
    }
}