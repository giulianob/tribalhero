#region

using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;

#endregion

namespace Game.Battle
{
    /// <summary>
    /// Manages the order of the attack. This implementation simply
    /// returns the next unit in the combat list that hasnt participated
    /// in the round. It doesn't care if its the same group or not.
    /// </summary>
    class BattleOrder
    {
        /// <summary>
        /// Returns the next object from the primary group that should attack.
        /// If primary group has no one able to attack, it will look into the secondary group instead.
        /// </summary>
        /// <returns>True if got an object from the current round. False if had to look into next round.</returns>
        public bool NextObject(uint round, List<CombatGroup> attacker, List<CombatGroup> defender, BattleManager.BattleSide sideAttacking, out CombatObject outCombatObject, out CombatGroup outCombatGroup, out BattleManager.BattleSide foundInGroup)
        {
            var offensiveCombatList = sideAttacking == BattleManager.BattleSide.Attack ? attacker : defender;
            var defensiveCombatList = sideAttacking == BattleManager.BattleSide.Attack ? defender : attacker;
            var offensiveSide = sideAttacking;
            var defensiveSide = sideAttacking == BattleManager.BattleSide.Attack ? BattleManager.BattleSide.Defense : BattleManager.BattleSide.Attack;

            // Look into offenside combat list first
            CombatObject outCombatObjectAttacker;
            CombatGroup outCombatGroupAttacker;
            if (NextObjectFromList(round, offensiveCombatList, out outCombatObjectAttacker, out outCombatGroupAttacker))
            {
                foundInGroup = offensiveSide;
                outCombatGroup = outCombatGroupAttacker;
                outCombatObject = outCombatObjectAttacker;
                return true;
            }

            // Couldnt find in the attacker so look in defense
            CombatObject outCombatObjectDefender;
            CombatGroup outCombatGroupDefender;
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
                foundInGroup = BattleManager.BattleSide.Attack;
                return true;
            }

            return false;
        }

        private bool NextObjectFromList(uint round, List<CombatGroup> combatGroups, out CombatObject outObj, out CombatGroup outGroup)
        {
            // Find any objects that are still in the current round
            foreach (CombatGroup combatGroup in combatGroups)
            {
                outObj = combatGroup.FirstOrDefault(obj => obj.LastRound == round);

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
            foreach (CombatGroup combatGroup in combatGroups)
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