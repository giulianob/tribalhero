using Game.Setup.DependencyInjection;

namespace Game.Map
{
    public class RegionObjectListFactory : IRegionObjectListFactory
    {
        private readonly IKernel kernel;

        public RegionObjectListFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public RegionObjectList CreateRegionObjectList()
        {
            return new RegionObjectList(kernel.Get<IRegionLocator>());
        }
    }
}