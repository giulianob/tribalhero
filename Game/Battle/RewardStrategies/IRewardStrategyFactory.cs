using Game.Data;

namespace Game.Battle.RewardStrategies
{
    public interface IRewardStrategyFactory
    {
        CityRewardStrategy CreateCityRewardStrategy(ICity city);
    }
}
