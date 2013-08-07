using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Setup;
using Persistance;

namespace Game.Battle
{
    public class PublicBattleManager : BattleManager
    {
        public PublicBattleManager(uint battleId,
                                   BattleLocation location,
                                   BattleOwner owner,
                                   IRewardStrategy rewardStrategy,
                                   IDbManager dbManager,
                                   IBattleReport battleReport,
                                   ICombatListFactory combatListFactory,
                                   BattleFormulas battleFormulas,
                                   IBattleOrder battleOrder,
                                   IBattleRandom battleRandom)
            : base(battleId, location, owner, rewardStrategy, dbManager, battleReport, combatListFactory, battleFormulas, battleOrder, battleRandom)
        {
        }

        public override Error CanWatchBattle(Data.IPlayer player, out System.Collections.Generic.IEnumerable<string> errorParams)
        {
            var canWatchBattle = base.CanWatchBattle(player, out errorParams);

            if (canWatchBattle == Error.BattleViewableInRounds)
            {
                errorParams = new string[] {};
                return Error.Ok;
            }

            return canWatchBattle;
        }
    }
}