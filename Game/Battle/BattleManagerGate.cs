using System.Collections.Generic;
using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Data;
using Game.Data.Stronghold;
using Game.Setup;
using Persistance;

namespace Game.Battle
{
    public class BattleManagerGate : BattleManager
    {
        private readonly IStronghold stronghold;

        public BattleManagerGate(uint battleId,
                                 IStronghold stronghold,
                                 BattleLocation location,
                                 BattleOwner owner,
                                 IRewardStrategy rewardStrategy,
                                 IDbManager dbManager,
                                 IBattleReport battleReport,
                                 ICombatListFactory combatListFactory,
                                 BattleFormulas battleFormulas)
                : base(battleId, location, owner, rewardStrategy, dbManager, battleReport, combatListFactory, battleFormulas)
        {
            this.stronghold = stronghold;
        }

        public override Error CanWatchBattle(IPlayer player, out IEnumerable<string> errorParams)
        {
            var canWatchBattle = base.CanWatchBattle(player, out errorParams);

            if (canWatchBattle != Error.Ok)
            {
                return canWatchBattle;
            }

            // Since the gate battle isn't really viewable, we just return an error message that includes the gate HP
            errorParams = new[] {stronghold.Gate.ToString()};
            return Error.BattleNotViewableGate;
        }
    }
}