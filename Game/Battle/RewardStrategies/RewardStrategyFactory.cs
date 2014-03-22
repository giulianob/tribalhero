using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup.DependencyInjection;
using Game.Util.Locking;

namespace Game.Battle.RewardStrategies
{
    public class RewardStrategyFactory : IRewardStrategyFactory
    {
        private readonly IKernel kernel;

        public RewardStrategyFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public CityRewardStrategy CreateCityRewardStrategy(ICity city)
        {
            return new CityRewardStrategy(city, kernel.Get<IBattleFormulas>(), kernel.Get<Formula>(), kernel.Get<ILocker>());
        }

        public StrongholdRewardStrategy CreateStrongholdRewardStrategy(IStronghold stronghold)
        {
            return new StrongholdRewardStrategy(stronghold, kernel.Get<IGameObjectLocator>());
        }

        public BarbarianTribeRewardStrategy CreateBarbarianTribeRewardStrategy(IBarbarianTribe barbarianTribe)
        {
            return new BarbarianTribeRewardStrategy(barbarianTribe, kernel.Get<IBattleFormulas>());
        }
    }
}