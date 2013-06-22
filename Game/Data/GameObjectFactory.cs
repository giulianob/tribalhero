using Game.Data.Stats;
using Game.Data.Troop;
using Game.Setup;
using Ninject;
using Persistance;

namespace Game.Data
{
    class GameObjectFactory : IGameObjectFactory
    {
        private readonly IKernel kernel;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly ITechnologyManagerFactory technologyManagerFactory;

        public GameObjectFactory(IKernel kernel, IStructureCsvFactory structureCsvFactory, ITechnologyManagerFactory technologyManagerFactory)
        {
            this.kernel = kernel;
            this.structureCsvFactory = structureCsvFactory;
            this.technologyManagerFactory = technologyManagerFactory;
        }

        public IStructure CreateStructure(uint cityId, uint structureId, ushort type, byte level, uint x, uint y)
        {
            var baseStats = structureCsvFactory.GetBaseStats(type, level);
            var technologyManager = technologyManagerFactory.CreateTechnologyManager(EffectLocation.Object, cityId, structureId);
            var structureProperties = new StructureProperties(cityId, structureId);

            var structure = new Structure(structureId,
                                          new StructureStats(baseStats),
                                          x,
                                          y,
                                          technologyManager,
                                          structureProperties,
                                          kernel.Get<IDbManager>());
            
            return structure;
        }

        public ITroopObject CreateTroopObject(uint id, ITroopStub stub, uint x, uint y)
        {
            return new TroopObject(id, stub, x, y, kernel.Get<IDbManager>());            
        }
    }
}