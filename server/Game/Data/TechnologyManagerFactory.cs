using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Data
{
    public class TechnologyManagerFactory : ITechnologyManagerFactory
    {
        private readonly IKernel kernel;

        public TechnologyManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ITechnologyManager CreateTechnologyManager(EffectLocation location, uint cityId, uint ownerId)
        {
            return new TechnologyManager(location, cityId, ownerId, kernel.Get<IDbManager>());
        }
    }
}