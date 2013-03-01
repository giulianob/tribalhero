using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Notifications;
using Ninject;
using Persistance;

namespace Game.Data.Stronghold
{
    class StrongholdFactory : IStrongholdFactory
    {
        private readonly IKernel kernel;

        public StrongholdFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IStronghold CreateStronghold(uint id, string name, byte level, uint x, uint y, decimal gate)
        {
            var actionWorker = new ActionWorker();
            var troopManager = new TroopManager();
            var notificationManager = new NotificationManager(actionWorker, kernel.Get<IDbManager>());

            var stronghold = new Stronghold(id,
                                            name,
                                            level,
                                            x,
                                            y,
                                            gate,
                                            kernel.Get<IDbManager>(),
                                            notificationManager,
                                            troopManager,
                                            actionWorker);

            actionWorker.LockDelegate = () => stronghold;
            actionWorker.Location = stronghold;

            troopManager.BaseStation = stronghold;

            return stronghold;
        }
    }
}