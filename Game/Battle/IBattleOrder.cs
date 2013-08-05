using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;

namespace Game.Battle
{
    public interface IBattleOrder
    {
        /// <summary>
        ///     Returns the next object from the primary group that should attack.
        ///     If primary group has no one able to attack, it will look into the secondary group instead.
        /// </summary>
        /// <returns>True if got an object from the current round. False if had to look into next round.</returns>
        bool NextObject(uint round,
                                        IEnumerable<ICombatGroup> attacker,
                                        IEnumerable<ICombatGroup> defender,
                                        BattleManager.BattleSide sideAttacking,
                                        out ICombatObject outCombatObject,
                                        out ICombatGroup outCombatGroup,
                                        out BattleManager.BattleSide foundInGroup);
    }
}