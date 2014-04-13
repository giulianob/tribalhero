using System.Collections.Generic;
using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Data;
using Game.Data.Stronghold;
using Game.Map;
using Game.Setup;
using Persistance;

namespace Game.Battle
{
    public class StrongholdMainBattleManager : BattleManager
    {
        private readonly IGameObjectLocator gameObjectLocator;

        public StrongholdMainBattleManager(uint battleId,
                BattleLocation location,
                BattleOwner owner,
                IRewardStrategy rewardStrategy,
                IDbManager dbManager,
                IBattleReport battleReport,
                ICombatListFactory combatListFactory,
                IGameObjectLocator gameObjectLocator,
                IBattleOrder battleOrder,
                IBattleFormulas battleFormulas,                
                IBattleRandom random) :
                        base(battleId, location, owner, rewardStrategy, dbManager, battleReport, combatListFactory, battleFormulas, battleOrder, random)
        {
            this.gameObjectLocator = gameObjectLocator;
        }
            
        public override Error CanWatchBattle(IPlayer player, out IEnumerable<string> errorParams)
        {
            errorParams = new string[0];

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(Location.Id, out stronghold))
            {
                return Error.BattleNotViewable;
            }

            if (stronghold.StrongholdState == StrongholdState.Occupied && player.IsInTribe && stronghold.Tribe.Id == player.Tribesman.Tribe.Id)
            {
                return Error.Ok;
            }

            if (stronghold.GateOpenTo != null && player.IsInTribe && stronghold.GateOpenTo.Id == player.Tribesman.Tribe.Id)
            {
                return Error.Ok;
            }

            return Error.BattleNotViewable;
        }
    }
}