using Game.Data;

namespace Game.Battle
{
    public interface IBattleManagerFactory
    {
        IBattleManager CreateBattleManager(ICity owner);
    }
}
