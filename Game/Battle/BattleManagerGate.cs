using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private readonly decimal precision;

        public BattleManagerGate(uint battleId,
                                 IStronghold stronghold,
                                 BattleLocation location,
                                 BattleOwner owner,
                                 IRewardStrategy rewardStrategy,
                                 IDbManager dbManager,
                                 IBattleReport battleReport,
                                 ICombatListFactory combatListFactory,
                                 IBattleFormulas battleFormulas,
                                 IBattleOrder battleOrder,
                                 IBattleRandom battleRandom)
                : base(battleId, location, owner, rewardStrategy, dbManager, battleReport, combatListFactory, battleFormulas, battleOrder, battleRandom)
        {
            this.stronghold = stronghold;

            precision = 1m + (new Random((int)(owner.Id + battleId + location.Id)).Next(-20, 20) / 100m);
        }

        public override Error CanWatchBattle(IPlayer player, out IEnumerable<string> errorParams)
        {
            // Owner of stronghold gets to peek at the # of units attacking
            if (stronghold.Tribe != null && player.IsInTribe && player.Tribesman.Tribe == stronghold.Tribe)
            {
                var attackingUnits = Attackers.AllCombatObjects().Sum(x => x.Count);
                if (attackingUnits >= 100)
                {
                    attackingUnits = (int)(Math.Round(attackingUnits * precision / 10) * 10);                    
                }

                errorParams = new[] {stronghold.Gate.ToString(CultureInfo.InvariantCulture), attackingUnits.ToString(CultureInfo.InvariantCulture)};
                return Error.BattleViewableGateAttackingUnits;
            }

            var canWatchBattle = base.CanWatchBattle(player, out errorParams);

            if (canWatchBattle != Error.Ok)
            {
                return canWatchBattle;
            }

            // Since the gate battle isn't really viewable, we just return an error message that includes the gate HP
            errorParams = new[] {stronghold.Gate.ToString(CultureInfo.InvariantCulture)};
            return Error.BattleViewableGateHp;
        }
    }
}