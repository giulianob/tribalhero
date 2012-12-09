using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Data;
using Game.Setup;
using Persistance;

namespace Game.Battle
{
    public class BattleManagerPrivate : BattleManager
    {
        public BattleManagerPrivate(uint battleId,
                                    BattleLocation location,
                                    BattleOwner owner,
                                    IRewardStrategy rewardStrategy,
                                    IDbManager dbManager,
                                    IBattleReport battleReport,
                                    ICombatListFactory combatListFactory,
                                    BattleFormulas battleFormulas)
                : base(
                        battleId,
                        location,
                        owner,
                        rewardStrategy,
                        dbManager,
                        battleReport,
                        combatListFactory,
                        battleFormulas)
        {
        }

        public override Error CanWatchBattle(IPlayer player, out int roundsLeft)
        {
            roundsLeft = -1;
            return Error.BattleNotViewable;
        }
    }
}