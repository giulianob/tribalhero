using System;
using Game.Data;
using Game.Setup.DependencyInjection;
using Game.Util.Locking;
using Persistance;

namespace Game.Logic
{
    public class ActionWorkerFactory : IActionWorkerFactory
    {
        private readonly IKernel kernel;

        public ActionWorkerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IActionWorker CreateActionWorker(Func<ILockable> lockDelegate, ILocation location)
        {
            return new ActionWorker(lockDelegate,
                                    location,
                                    kernel.Get<ILocker>(),
                                    kernel.Get<IScheduler>(),
                                    kernel.Get<IDbManager>(),
                                    kernel.Get<RequirementFormula>());
        }
    }
}