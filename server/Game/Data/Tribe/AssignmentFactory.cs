using System;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup.DependencyInjection;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Tribe
{
    public class AssignmentFactory : IAssignmentFactory
    {
        private readonly IKernel kernel;

        public AssignmentFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public Assignment CreateAssignment(ITribe tribe, uint x, uint y, ILocation target, AttackMode mode, DateTime targetTime, string description, bool isAttack)
        {
            return new Assignment(tribe,
                                  x,
                                  y,
                                  target,
                                  mode,
                                  targetTime,
                                  description,
                                  isAttack,
                                  kernel.Get<Formula>(),
                                  kernel.Get<IDbManager>(),
                                  kernel.Get<IGameObjectLocator>(),
                                  kernel.Get<IScheduler>(),
                                  kernel.Get<Procedure>(),
                                  kernel.Get<ITileLocator>(),
                                  kernel.Get<IActionFactory>(),
                                  kernel.Get<ILocker>(),
                                  kernel.Get<ITroopObjectInitializerFactory>());
        }

        public Assignment CreateAssignmentFromDb(int id, ITribe tribe, uint x, uint y, ILocation target, AttackMode mode, DateTime targetTime, uint dispatchCount, string description, bool isAttack)
        {
            return new Assignment(id,
                                  tribe,
                                  x,
                                  y,
                                  target,
                                  mode,
                                  targetTime,
                                  dispatchCount,
                                  description,
                                  isAttack,
                                  kernel.Get<Formula>(),
                                  kernel.Get<IDbManager>(),
                                  kernel.Get<IGameObjectLocator>(),
                                  kernel.Get<IScheduler>(),
                                  kernel.Get<Procedure>(),
                                  kernel.Get<ITileLocator>(),
                                  kernel.Get<IActionFactory>(),
                                  kernel.Get<ILocker>(),
                                  kernel.Get<ITroopObjectInitializerFactory>());
        }
    }
}