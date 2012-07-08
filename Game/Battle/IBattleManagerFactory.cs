using Game.Data;

namespace Game.Battle
{
    public interface IBattleManagerFactory
    {
        IBattleManager CreateBattleManager(uint battleId, ICity city);
        IBattleManager CreateBattleManager(ICity city);
    }
}
