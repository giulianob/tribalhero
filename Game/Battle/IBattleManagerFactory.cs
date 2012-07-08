using Game.Data;

namespace Game.Battle
{
    public interface IBattleManagerFactory
    {
        IBattleManager CreateBattleManager(uint battleId, BattleLocation location, BattleOwner owner, ICity city);
        IBattleManager CreateBattleManager(BattleLocation location, BattleOwner owner, ICity city);
    }
}
