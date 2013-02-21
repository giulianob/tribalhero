using System;
using System.ComponentModel;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;

namespace Game.Battle
{
    public class StaminaMonitor
    {
        private short stamina;

        public StaminaMonitor(IBattleManager battleManager,
                              ICombatGroup combatGroup,
                              short initialStamina,
                              BattleFormulas battleFormulas)
        {
            CombatGroup = combatGroup;
            Stamina = initialStamina;
            BattleFormulas = battleFormulas;

            battleManager.ActionAttacked += BattleActionAttacked;
            battleManager.WithdrawAttacker += BattleWithdrawAttacker;
            battleManager.EnterRound += BattleEnterRound;
            battleManager.ExitTurn += BattleExitTurn;
        }

        private ICombatGroup CombatGroup { get; set; }

        public short Stamina
        {
            get
            {
                return stamina;
            }
            set
            {
                stamina = Math.Max(value, (short)0);
                PropertyChanged(this, new PropertyChangedEventArgs("Stamina"));
            }
        }

        private BattleFormulas BattleFormulas { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void BattleActionAttacked(IBattleManager battle,
                                          BattleManager.BattleSide attackingside,
                                          ICombatGroup attackerGroup,
                                          ICombatObject attacker,
                                          ICombatGroup targetGroup,
                                          ICombatObject target,
                                          decimal damage)
        {
            if (attackingside == BattleManager.BattleSide.Attack && target.ClassType == BattleClass.Structure && target.IsDead)
            {
                Stamina = BattleFormulas.GetStaminaStructureDestroyed(Stamina, target);
            }
        }

        private void BattleWithdrawAttacker(IBattleManager battle, ICombatGroup groupWithdrawn)
        {
            if (groupWithdrawn != CombatGroup)
            {
                return;
            }

            battle.ActionAttacked -= BattleActionAttacked;
            battle.WithdrawAttacker -= BattleWithdrawAttacker;
            battle.EnterRound -= BattleEnterRound;
            battle.ExitTurn -= BattleExitTurn;
        }

        private void BattleEnterRound(IBattleManager battle, ICombatList attackers, ICombatList defenders, uint round)
        {
            // Reduce stamina and check if we need to remove this stub
            Stamina -= 1;

            if (Stamina == 0)
            {
                battle.Remove(CombatGroup, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);
            }
        }

        private void BattleExitTurn(IBattleManager battle, ICombatList attackers, ICombatList defenders, int turn)
        {
            // Remove troop from battle if he is out of stamina, we need to check here because he might have lost
            // some stamina after knocking down a building
            if (Stamina == 0)
            {
                battle.Remove(CombatGroup, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);
            }
        }
    }
}