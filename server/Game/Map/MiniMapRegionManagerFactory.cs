using Game.Setup.DependencyInjection;

namespace Game.Map
{
    public class MiniMapRegionManagerFactory : IMiniMapRegionManagerFactory
    {
        private readonly IKernel kernel;

        public MiniMapRegionManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IMiniMapRegionManager CreateMiniMapRegionManager(int regionCount)
        {
            return new MiniMapRegionManager(regionCount, kernel.Get<IMiniMapRegionFactory>());
        }
    }
}