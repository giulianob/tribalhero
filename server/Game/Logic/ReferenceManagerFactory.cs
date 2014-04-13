using Game.Setup.DependencyInjection;
using Game.Util.Locking;
using Persistance;

namespace Game.Logic
{
    public class ReferenceManagerFactory : IReferenceManagerFactory
    {
        private readonly IKernel kernel;

        public ReferenceManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IReferenceManager CreateReferenceManager(uint cityId, IActionWorker actionWorker, ILockable lockingObj)
        {
            return new ReferenceManager(cityId, actionWorker, lockingObj, kernel.Get<IDbManager>(), kernel.Get<ILocker>());
        }
    }
}