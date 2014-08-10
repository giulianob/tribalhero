using Game.Data;
using Game.Map;
using Game.Setup.DependencyInjection;

namespace Game.Module.Remover
{
    public class PlayerSelectorFactory : IPlayerSelectorFactory
    {
        private readonly IKernel kernel;

        public PlayerSelectorFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public NewbieIdleSelector CreateNewbieIdleSelector()
        {
            return new NewbieIdleSelector();
        }

        public AllButYouSelector CreateAllButYouSelector(IPlayer you)
        {
            return new AllButYouSelector(you, kernel.Get<IWorld>());
        }
    }
}