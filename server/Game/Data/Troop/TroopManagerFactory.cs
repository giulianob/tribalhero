using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Data.Troop
{
    public class TroopManagerFactory : ITroopManagerFactory
    {
        private readonly IKernel kernel;

        public TroopManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ITroopManager CreateTroopManager()
        {
            return new TroopManager(kernel.Get<IDbManager>());
        }
    }
}