#region

using System;
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
        private readonly IBattleRandom random;

        public BattleOrder(IBattleRandom random)
        {
            this.random = random;
        }

        /// <summary>
        ///     Returns the next object from the primary group that should attack.
        ///     If primary group has no one able to attack, it will look into the secondary group instead.
        /// </summary>
        /// <returns>True if got an object from the current round. False if had to look into next round.</returns>
        public bool NextObject(uint round,
                               ICombatList attacker,
                               ICombatList defender,
                               out ICombatObject outCombatObject,
                               out ICombatGroup outCombatGroup,
                               out BattleManager.BattleSide foundInGroup)
        {
            var attackerUpkeep = attacker.UpkeepNotParticipated(round);
            var defenderUpkeep = defender.UpkeepNotParticipated(round);
            if (attackerUpkeep == 0 && defenderUpkeep == 0)
            {
                attackerUpkeep = attacker.UpkeepNotParticipated(round + 1);
                defenderUpkeep = defender.UpkeepNotParticipated(round + 1);
            }
            BattleManager.BattleSide sideAttack = random.Next(attackerUpkeep + defenderUpkeep) < attackerUpkeep
                                                          ? BattleManager.BattleSide.Attack
                                                          : BattleManager.BattleSide.Defense;
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
    }
}