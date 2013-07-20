#region

using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Util;
#endregion

namespace Game.Battle
{
    /// <summary>
    ///     Manages the order of the attack. This implementation simply
    ///     returns the next unit in the combat list that hasnt participated
    ///     in the round. It doesn't care if its the same group or not.
    /// </summary>
    public class BattleOrder : IBattleOrder
    {
        /// <summary>
        ///     Returns the next object from the primary group that should attack.
        ///     If primary group has no one able to attack, it will look into the secondary group instead.
        /// </summary>
        /// <returns>True if got an object from the current round. False if had to look into next round.</returns>
        public bool NextObject(uint round,
                               uint turn,
                               ICombatList attacker,
                               ICombatList defender,
                               out ICombatObject outCombatObject,
                               out ICombatGroup outCombatGroup,
                               out BattleManager.BattleSide foundInGroup)
        {
            var sideAttack = NextSideAttacking(turn, attacker.UpkeepNotParticipated(round), defender.UpkeepNotParticipated(round));
            var offensiveCombatList = sideAttack == BattleManager.BattleSide.Attack ? attacker : defender;
            var defensiveCombatList = sideAttack == BattleManager.BattleSide.Attack ? defender : attacker;
            var offensiveSide = sideAttack;
            var defensiveSide = sideAttack == BattleManager.BattleSide.Attack
                                        ? BattleManager.BattleSide.Defense
                                        : BattleManager.BattleSide.Attack;

            // Look into offenside combat list first
            ICombatObject outCombatObjectAttacker;
            ICombatGroup outCombatGroupAttacker;
            if (NextObjectFromList(round, offensiveCombatList, out outCombatObjectAttacker, out outCombatGroupAttacker))
            {
                foundInGroup = offensiveSide;
                outCombatGroup = outCombatGroupAttacker;
                outCombatObject = outCombatObjectAttacker;
                return true;
            }

            // Couldnt find in the attacker so look in defense
            ICombatObject outCombatObjectDefender;
            ICombatGroup outCombatGroupDefender;
            if (NextObjectFromList(round, defensiveCombatList, out outCombatObjectDefender, out outCombatGroupDefender))
            {
                foundInGroup = defensiveSide;
                outCombatGroup = outCombatGroupDefender;
                outCombatObject = outCombatObjectDefender;
                return true;
            }

            // Okay looks like both sides are done for this round. If we had an attacker
            // then we return that, otherwise go to the defender
            if (outCombatObjectAttacker != null)
            {
                foundInGroup = offensiveSide;
                outCombatGroup = outCombatGroupAttacker;
                outCombatObject = outCombatObjectAttacker;
            }
            else if (outCombatObjectDefender != null)
            {
                foundInGroup = defensiveSide;
                outCombatGroup = outCombatGroupDefender;
                outCombatObject = outCombatObjectDefender;
            }
                    // If this happens then it means there is no one in the battle or the battle is prolly over
            else
            {
                outCombatGroup = null;
                outCombatObject = null;
                foundInGroup = offensiveSide;
                return true;
            }

            return false;
        }

        private bool NextObjectFromList(uint round,
                                        IEnumerable<ICombatGroup> combatGroups,
                                        out ICombatObject outObj,
                                        out ICombatGroup outGroup)
        {
            // Find any objects that are still in the current round
            foreach (ICombatGroup combatGroup in combatGroups)
            {
                outObj = combatGroup.FirstOrDefault(obj => obj.LastRound <= round);

                if (outObj == null)
                {
                    continue;
                }

                // We've found an object thats still in the current round
                // so we're done
                outGroup = combatGroup;
                return true;
            }

            // No object in the current round, get the first one for the next round
            foreach (ICombatGroup combatGroup in combatGroups)
            {
                outObj = combatGroup.FirstOrDefault(obj => obj.LastRound == round + 1);

                if (outObj == null)
                {
                    continue;
                }

                // Found an object in the next round
                outGroup = combatGroup;
                return false;
            }

            // Couldnt find anything, battle is probably over
            outObj = null;
            outGroup = null;
            return false;
        }

        private BattleManager.BattleSide NextSideAttacking(uint turn, int attackerUpkeep, int defenderUpkeep)
        {
            /* I have to use the inverse ratio because i wanted the extra hits happening at the end of the cycle
             * 50(atk) vs 200(def), ratio is 0.25, the 1 is the base %.
             * Assume that turn is 0 based, we need to add 1 for the logic to work
             * if (turn +1) % (ratio+1)< 1
             *      then the side with higher count attacks
             * turn 0 = 1.00 attacker
             * turn 1 = 0.75 defender
             * turn 2 = 0.50 defender
             * turn 3 = 0.25 defender
             * turn 4 = 0.00 defender
             * turn 5 = 1.00 attacker
             */
            var sideWithMoreUnits = attackerUpkeep >= defenderUpkeep ? BattleManager.BattleSide.Attack : BattleManager.BattleSide.Defense;
            var sideWithLessUnits = attackerUpkeep >= defenderUpkeep ? BattleManager.BattleSide.Defense : BattleManager.BattleSide.Attack;
            double ratio = (attackerUpkeep > defenderUpkeep ? (double)defenderUpkeep / attackerUpkeep : (double)attackerUpkeep / defenderUpkeep);
            return (turn + 1) % (ratio + 1) < 1 ? sideWithMoreUnits : sideWithLessUnits;
        }
    }
}