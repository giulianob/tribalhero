using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Notifications;
using Game.Logic.Procedures;
using Game.Map;
using Ninject;
using Persistance;

namespace Game.Data
{
    public class CityFactory : ICityFactory
    {
        private readonly IKernel kernel;

        private readonly Procedure procedure;

        private readonly IActionWorkerFactory actionWorkerFactory;

        private readonly INotificationManagerFactory notificationManagerFactory;

        private readonly IReferenceManagerFactory referenceManagerFactory;

        private readonly ITechnologyManagerFactory technologyManagerFactory;

        private readonly ITroopManagerFactory troopManagerFactory;

        private readonly IUnitTemplateFactory unitTemplateFactory;

        public CityFactory(IKernel kernel,
                           Procedure procedure,
                           IActionWorkerFactory actionWorkerFactory,
                           INotificationManagerFactory notificationManagerFactory,
                           IReferenceManagerFactory referenceManagerFactory,
                           ITechnologyManagerFactory technologyManagerFactory,
                           ITroopManagerFactory troopManagerFactory,
                           IUnitTemplateFactory unitTemplateFactory)
        {
            this.kernel = kernel;
            this.procedure = procedure;
            this.actionWorkerFactory = actionWorkerFactory;
            this.notificationManagerFactory = notificationManagerFactory;
            this.referenceManagerFactory = referenceManagerFactory;
            this.technologyManagerFactory = technologyManagerFactory;
            this.troopManagerFactory = troopManagerFactory;
            this.unitTemplateFactory = unitTemplateFactory;
        }

        public ICity CreateCity(uint id, IPlayer owner, string name, Resource resource, byte radius, decimal ap)
        {
            return CreateCity(id,
                              owner,
                              name,
                              new LazyResource(crop: resource.Crop,
                                               gold: resource.Gold,
                                               iron: resource.Iron,
                                               wood: resource.Wood,
                                               labor: resource.Labor),
                              radius,
                              ap);
        }

        public ICity CreateCity(uint id, IPlayer owner, string name, LazyResource resource, byte radius, decimal ap)
        {
            var worker = actionWorkerFactory.CreateActionWorker(() => owner, new SimpleLocation(LocationType.City, id));
            var notifications = notificationManagerFactory.CreateCityNotificationManager(worker, id);
            var references = referenceManagerFactory.CreateReferenceManager(id, worker, owner);
            var technologies = technologyManagerFactory.CreateTechnologyManager(EffectLocation.City, id, id);
            var troops = troopManagerFactory.CreateTroopManager();
            var template = unitTemplateFactory.CreateUnitTemplate(id);
            var troopStubFactory = new CityTroopStubFactory();

            var city = new City(id,
                                owner,
                                name,
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
                                kernel.Get<ICityRegionManager>(),
                                kernel.Get<IRegionManager>(),
                                kernel.Get<IGameObjectFactory>(),
                                kernel.Get<Procedure>(),
                                kernel.Get<Formula>());

            // TODO: We should figure a cleaner way so we dont need to have this circular dependency
            troops.BaseStation = city;
            troopStubFactory.City = city;

            procedure.RecalculateCityResourceRates(city);
            procedure.SetResourceCap(city);

            return city;
        }
    }
}