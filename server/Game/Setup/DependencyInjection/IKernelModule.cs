using SimpleInjector;

namespace Game.Setup.DependencyInjection
{
    public interface IKernelModule
    {
        void Load(Container container);
    }
}