using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;

namespace Game.Battle
{
    public interface IBattleManagerFactory
    {
        IBattleManager CreateBattleManager(uint battleId,
                                           BattleLocation battleLocation,
                                           BattleOwner battleOwner,
                                           ICity city);

        IBattleManager CreateBattleManager(BattleLocation location, BattleOwner owner, ICity city);

        IBattleManager CreateStrongholdMainBattleManager(uint battleId,
                                                         BattleLocation battleLocation,
                                                         BattleOwner battleOwner,
                                                         IStronghold stronghold);

        IBattleManager CreateStrongholdMainBattleManager(BattleLocation battleLocation,
                                                         BattleOwner battleOwner,
                                                         IStronghold stronghold);

        IBattleManager CreateStrongholdGateBattleManager(uint battleId,
                                                         BattleLocation battleLocation,
                                                         BattleOwner battleOwner,
                                                         IStronghold stronghold);

        IBattleManager CreateStrongholdGateBattleManager(BattleLocation battleLocation,
                                                         BattleOwner battleOwner,
                                                         IStronghold stronghold);

        IBattleManager CreateBarbarianBattleManager(BattleLocation battleLocation, BattleOwner battleOwner, IBarbarianTribe barbarianTribe);

        IBattleManager CreateBarbarianBattleManager(uint battleId,
                                                    BattleLocation battleLocation,
                                                    BattleOwner battleOwner,
                                                    IBarbarianTribe barbarianTribe);
    }
}