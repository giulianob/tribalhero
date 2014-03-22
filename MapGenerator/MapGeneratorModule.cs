using Game.Setup.DependencyInjection;
using SimpleInjector;

namespace MapGenerator
{
    public class MapGeneratorModule : IKernelModule
    {
        public void Load(Container container)
        {
            container.Register<MapGenerator>();
        }
    }
}