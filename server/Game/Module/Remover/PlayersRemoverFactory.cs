using Game.Logic;
using Game.Setup.DependencyInjection;
using Game.Util.Locking;

namespace Game.Module.Remover
{
    public class PlayersRemoverFactory : IPlayersRemoverFactory
    {
        private readonly IKernel kernel;

        public PlayersRemoverFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public PlayersRemover CreatePlayersRemover(IPlayerSelector playerSelector)
        {
            return new PlayersRemover(playerSelector, kernel.Get<ICityRemoverFactory>(), kernel.Get<IScheduler>(), kernel.Get<ILocker>());
        }
    }
}