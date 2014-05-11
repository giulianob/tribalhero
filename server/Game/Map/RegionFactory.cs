using Game.Setup.DependencyInjection;
using Game.Util.Locking;

namespace Game.Map
{
    public class RegionFactory : IRegionFactory
    {
        private readonly IKernel kernel;

        public RegionFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IRegion CreateRegion(byte[] map)
        {
            return new Region(map, kernel.Get<DefaultMultiObjectLock.Factory>(), kernel.Get<IRegionLocator>(), kernel.Get<IRegionObjectListFactory>());
        }
    }
}