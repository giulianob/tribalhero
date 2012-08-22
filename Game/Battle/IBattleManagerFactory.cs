using Game.Data;
using Game.Data.Stronghold;

namespace Game.Battle
{
    public interface IBattleManagerFactory
    {
        IBattleManager CreateBattleManager(uint battleId, BattleLocation battleLocation, BattleOwner battleOwner, ICity city);

        IBattleManager CreateBattleManager(BattleLocation location, BattleOwner owner, ICity city);

        IBattleManager CreateBattleManager(uint battleId, BattleLocation battleLocation, BattleOwner battleOwner, IStronghold stronghold);

        IBattleManager CreateBattleManager(BattleLocation battleLocation, BattleOwner battleOwner, IStronghold stronghold);
    }
}
