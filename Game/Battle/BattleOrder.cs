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
        public decimal Meter { get; set; }

        public BattleOrder(int meter = 0)
        {
            Meter = meter;
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
            BattleManager.BattleSide sideAttack;
            if (Meter == 0)
            {
                sideAttack = attacker.UpkeepNotParticipated(round) < defender.UpkeepNotParticipated(round)
                                     ? BattleManager.BattleSide.Attack
                                     : BattleManager.BattleSide.Defense;
            }
            else
            {
                sideAttack = Meter > 0 ? BattleManager.BattleSide.Defense : BattleManager.BattleSide.Attack;
            }
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
                UpdateMeter(round, foundInGroup, offensiveCombatList, outCombatObject);
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
                UpdateMeter(round, foundInGroup, defensiveCombatList, outCombatObject);
                return true;
            }

            // Okay looks like both sides are done for this round. If we had an attacker
            // then we return that, otherwise go to the defender
            if (outCombatObjectAttacker != null)
            {
                foundInGroup = offensiveSide;
                outCombatGroup = outCombatGroupAttacker;
                outCombatObject = outCombatObjectAttacker;
                UpdateMeter(round + 1, foundInGroup, foundInGroup == BattleManager.BattleSide.Attack ? attacker : defender, outCombatObject);
            }
            else if (outCombatObjectDefender != null)
            {
                foundInGroup = defensiveSide;
                outCombatGroup = outCombatGroupDefender;
                outCombatObject = outCombatObjectDefender;
                UpdateMeter(round + 1, foundInGroup, foundInGroup == BattleManager.BattleSide.Attack ? attacker : defender, outCombatObject);
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

        private void UpdateMeter(uint round, BattleManager.BattleSide foundInGroup, ICombatList combatList, ICombatObject combatObject)
        {
            /*
             * when meter is 0 or higher defender hits
             * when a defender hits, the meter go down by it's %
             * when an attacker hits, the meter go up by it's %
             */
            var totalUpkeep = combatList.UpkeepNotParticipated(round);
            if (totalUpkeep <= 0)
                throw new Exception("How can this happen!");

            if (foundInGroup == BattleManager.BattleSide.Defense)
            {
                Meter -= (decimal)combatObject.Upkeep / totalUpkeep;
            }
            else
            {
                Meter += (decimal)combatObject.Upkeep / totalUpkeep;
            }
        }
    }
}