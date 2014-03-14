using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Notifications;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Data
{
    public class CityFactory : ICityFactory
    {
        private readonly IKernel kernel;

        private readonly IActionWorkerFactory actionWorkerFactory;

        private readonly INotificationManagerFactory notificationManagerFactory;

        private readonly IReferenceManagerFactory referenceManagerFactory;

        private readonly ITechnologyManagerFactory technologyManagerFactory;

        private readonly ITroopManagerFactory troopManagerFactory;

        private readonly IUnitTemplateFactory unitTemplateFactory;

        public CityFactory(IKernel kernel,
                           IActionWorkerFactory actionWorkerFactory,
                           INotificationManagerFactory notificationManagerFactory,
                           IReferenceManagerFactory referenceManagerFactory,
                           ITechnologyManagerFactory technologyManagerFactory,
                           ITroopManagerFactory troopManagerFactory,
                           IUnitTemplateFactory unitTemplateFactory)
        {
            this.kernel = kernel;
            this.actionWorkerFactory = actionWorkerFactory;
            this.notificationManagerFactory = notificationManagerFactory;
            this.referenceManagerFactory = referenceManagerFactory;
            this.technologyManagerFactory = technologyManagerFactory;
            this.troopManagerFactory = troopManagerFactory;
            this.unitTemplateFactory = unitTemplateFactory;
        }

        public ICity CreateCity(uint id, IPlayer owner, string name, Position position, Resource resource, byte radius, decimal ap)
        {
            return CreateCity(id,
                              owner,
                              name, 
                              position,
                              new LazyResource(crop: resource.Crop,
                                               gold: resource.Gold,
                                               iron: resource.Iron,
                                               wood: resource.Wood,
                                               labor: resource.Labor),
                              radius,
                              ap);
        }

        public ICity CreateCity(uint id, IPlayer owner, string name, Position position, ILazyResource resource, byte radius, decimal ap)
        {
            var worker = actionWorkerFactory.CreateActionWorker(() => owner, new SimpleLocation(LocationType.City, id));
            var notifications = notificationManagerFactory.CreateCityNotificationManager(worker, id, "/PLAYER/" + owner.PlayerId);
            var references = referenceManagerFactory.CreateReferenceManager(id, worker, owner);
            var technologies = technologyManagerFactory.CreateTechnologyManager(EffectLocation.City, id, id);
            var troops = troopManagerFactory.CreateTroopManager();
            var template = unitTemplateFactory.CreateUnitTemplate(id);
            var troopStubFactory = new CityTroopStubFactory(kernel);

            var city = new City(id,
                                owner,
                                name,
                                position,
                                resource,
                                radius,
                                ap,
                                worker,
                                notifications,
                                references,
                                technologies,
                                troops,
                                template,
                                troopStubFactory,
                                kernel.Get<IDbManager>(),
                                kernel.Get<IGameObjectFactory>(),
                                kernel.Get<IActionFactory>(),
                                kernel.Get<BattleProcedure>());

            // TODO: We should figure a cleaner way so we dont need to have this circular dependency
            troops.BaseStation = city;
            troopStubFactory.City = city;

            return city;
        }
    }
}