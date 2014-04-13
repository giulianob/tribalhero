using Game.Data;
using Game.Setup.DependencyInjection;
using Game.Util.Locking;

namespace Game.Map
{
    public class MiniMapRegionFactory : IMiniMapRegionFactory
    {
        private readonly IKernel kernel;

        public MiniMapRegionFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public MiniMapRegion CreateMiniMapRegion()
        {
            return new MiniMapRegion(kernel.Get<DefaultMultiObjectLock.Factory>(), kernel.Get<IGlobal>());
        }
    }
}