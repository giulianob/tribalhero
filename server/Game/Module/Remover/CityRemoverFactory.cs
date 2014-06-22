using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Map;
using Game.Module.Remover;
using Game.Setup.DependencyInjection;
using Game.Util.Locking;
using Persistance;

namespace Game.Module
{
    public class CityRemoverFactory : ICityRemoverFactory
    {
        private readonly IKernel kernel;

        public CityRemoverFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public CityRemover CreateCityRemover(uint cityId)
        {
            return new CityRemover(cityId, kernel.Get<IActionFactory>(), kernel.Get<ITileLocator>(), kernel.Get<IWorld>(), kernel.Get<IScheduler>(), kernel.Get<ILocker>(), kernel.Get<IDbManager>(), kernel.Get<ITroopObjectInitializerFactory>());
        }
    }
}