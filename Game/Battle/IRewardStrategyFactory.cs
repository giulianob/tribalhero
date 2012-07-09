using Game.Data;

namespace Game.Battle
{
    public interface IRewardStrategyFactory
    {
        CityRewardStrategy CreateCityRewardStrategy(ICity city);
    }
}
