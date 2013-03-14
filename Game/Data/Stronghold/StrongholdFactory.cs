using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Notifications;
using Game.Map;
using Ninject;
using Persistance;

namespace Game.Data.Stronghold
{
    class StrongholdFactory : IStrongholdFactory
    {
        private readonly IKernel kernel;

        private readonly IActionWorkerFactory actionWorkerFactory;

        private readonly ITroopManagerFactory troopManagerFactory;

        private readonly INotificationManagerFactory notificationManagerFactory;

        public StrongholdFactory(IKernel kernel,
                                 IActionWorkerFactory actionWorkerFactory,
                                 ITroopManagerFactory troopManagerFactory,
                                 INotificationManagerFactory notificationManagerFactory)
        {
            this.kernel = kernel;
            this.actionWorkerFactory = actionWorkerFactory;
            this.troopManagerFactory = troopManagerFactory;
            this.notificationManagerFactory = notificationManagerFactory;
        }

        public IStronghold CreateStronghold(uint id, string name, byte level, uint x, uint y, decimal gate)
        {
            IStronghold stronghold;

            var actionWorker = actionWorkerFactory.CreateActionWorker(null, new SimpleLocation(LocationType.Stronghold, id));
            var troopManager = troopManagerFactory.CreateTroopManager();
            var notificationManager = notificationManagerFactory.CreateNotificationManager(actionWorker);

            stronghold = new Stronghold(id,
                                        name,
                                        level,
                                        x,
                                        y,
                                        gate,
                                        kernel.Get<IDbManager>(),
                                        notificationManager,
                                        troopManager,
                                        actionWorker,
                                        kernel.Get<Formula>(),
                                        kernel.Get<IRegionManager>());

            // TODO: Remove circular dependency
            actionWorker.LockDelegate = () => stronghold;            

            troopManager.BaseStation = stronghold;

            return stronghold;
        }
    }
}