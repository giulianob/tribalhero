using Ninject.Modules;

namespace MapGenerator
{
    public class MapGeneratorModule : NinjectModule
    {
        public override void Load()
        {
            Bind<MapGenerator>().ToSelf();
        }
    }
}