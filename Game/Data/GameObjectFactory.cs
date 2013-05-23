using Game.Data.Stats;
using Game.Map;
using Game.Setup;
using Ninject;
using Persistance;

namespace Game.Data
{
    class GameObjectFactory : IGameObjectFactory
    {
        private readonly IKernel kernel;

        private readonly StructureCsvFactory structureCsvFactory;

        private readonly ITechnologyManagerFactory technologyManagerFactory;

        public GameObjectFactory(IKernel kernel, StructureCsvFactory structureCsvFactory, ITechnologyManagerFactory technologyManagerFactory)
        {
            this.kernel = kernel;
            this.structureCsvFactory = structureCsvFactory;
            this.technologyManagerFactory = technologyManagerFactory;
        }

        public IStructure CreateStructure(uint cityId, uint structureId, ushort type, byte level)
        {
            var baseStats = structureCsvFactory.GetBaseStats(type, level);
            var technologyManager = technologyManagerFactory.CreateTechnologyManager(EffectLocation.Object, cityId, structureId);
            var structureProperties = new StructureProperties(cityId, structureId);

            var structure = new Structure(structureId,
                                          new StructureStats(baseStats),
                                          technologyManager,
                                          structureProperties,
                                          kernel.Get<IDbManager>(),
                                          kernel.Get<IRegionManager>());
            
            return structure;
        }
    }
}