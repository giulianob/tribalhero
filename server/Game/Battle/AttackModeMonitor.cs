using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data.Troop;
using Game.Setup;

namespace Game.Battle
{
    public class AttackModeMonitor
    {
        private readonly ICombatGroup combatGroup;

        private readonly ITroopStub troopStub;

        public AttackModeMonitor(IBattleManager battleManager, ICombatGroup combatGroup, ITroopStub troopStub)
        {
            this.troopStub = troopStub;
            this.combatGroup = combatGroup;

            battleManager.ActionAttacked += BattleActionAttacked;
            battleManager.WithdrawAttacker += BattleWithdrawAttacker;
        }

        private void BattleActionAttacked(IBattleManager battle,
                                          BattleManager.BattleSide attackingside,
                                          ICombatGroup attackerGroup,
                                          ICombatObject attacker,
                                          ICombatGroup targetGroup,
                                          ICombatObject target,
                                          decimal damage,
                                          int attackerCount,
                                          int targetCount)
        {
            // Check if the unit being attacked belongs to us
            if (targetGroup.Id != combatGroup.Id)
            {
                return;
            }

            // Check to see if player should retreat
            var remainingUnitCount = troopStub.TotalCount;

            // Don't return if we haven't fulfilled the minimum rounds or not below the threshold
            if (target.RoundsParticipated < Config.battle_retreat_min_rounds || remainingUnitCount == 0 ||
                remainingUnitCount > troopStub.RetreatCount)
            {
                return;
            }

            battle.Remove(combatGroup,
                          attackingside == BattleManager.BattleSide.Attack
                                  ? BattleManager.BattleSide.Defense
                                  : BattleManager.BattleSide.Attack,
                          ReportState.Retreating);
        }

        private void BattleWithdrawAttacker(IBattleManager battle, ICombatGroup groupWithdrawn)
        {
            if (groupWithdrawn != combatGroup)
            {
                return;
            }

            battle.ActionAttacked -= BattleActionAttacked;
            battle.WithdrawAttacker -= BattleWithdrawAttacker;
        }
    }
}