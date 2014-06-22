#region

using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;

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
            var attackerUpkeep = attacker.UpkeepNotParticipatedInRound(round);
            var defenderUpkeep = defender.UpkeepNotParticipatedInRound(round);
            if (attackerUpkeep == 0 && defenderUpkeep == 0)
            {
                attackerUpkeep = attacker.UpkeepNotParticipatedInRound(round + 1);
                defenderUpkeep = defender.UpkeepNotParticipatedInRound(round + 1);
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
                                        IList<ICombatGroup> combatGroups,
                                        out ICombatObject outObj,
                                        out ICombatGroup outGroup)
        {
            if (combatGroups.Count == 0)
            {
                outGroup = null;
                outObj = null;
                return false;
            }

            int startGroupIdx = random.Next(combatGroups.Count);            

            // Find any objects that are still in the current round
            for (var combatGroupIdx = 0; combatGroupIdx < combatGroups.Count; combatGroupIdx++)
            {
                var combatGroup = combatGroups[(startGroupIdx + combatGroupIdx) % combatGroups.Count];

                int startObjIdx = random.Next(combatGroup.Count);

                for (var combatObjIdx = 0; combatObjIdx < combatGroup.Count; combatObjIdx++)
                {
                    outObj = combatGroup[(startObjIdx + combatObjIdx) % combatGroup.Count];

                    if (outObj.IsWaitingToJoinBattle || outObj.HasAttacked(round))
                    {
                        continue;
                    }

                    // We've found an object thats still in the current round
                    // so we're done
                    outGroup = combatGroup;
                    return true;
                }
            }            
            
            // No object in the current round, get a random object from the group we got above
            // We loop incase the group is empty then it continues onto the next one
            for (var combatGroupIdx = 0; combatGroupIdx < combatGroups.Count; combatGroupIdx++)
            {
                var combatGroup = combatGroups[(startGroupIdx + combatGroupIdx) % combatGroups.Count];

                if (combatGroup.Count == 0)
                {
                    continue;
                }

                outGroup = combatGroup;
                outObj = combatGroup[random.Next(combatGroup.Count)];
                return false;
            }

            outGroup = null;
            outObj = null;
            return false;
        }
    }
}