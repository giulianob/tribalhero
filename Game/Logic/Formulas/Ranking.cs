#region

using Game.Data;
using Game.Data.Stats;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {

        /// <summary>
        /// Gets the ammount of attack points that should be received
        /// </summary>
        /// <param name="enemiesKilled">Value of enemy units killed</param>
        /// <param name="unitsKilled">Value of units in the troop killed</param>
        /// <returns></returns>
        public static int GetAttackPoint(int enemiesKilled, int unitsKilled) {
            return enemiesKilled * 2 - unitsKilled;
        }

        /// <summary>
        /// Gets the ammount of attack points the attacker should receive for killing an enemy.
        /// </summary>
        /// <param name="type">Type of unit killed</param>
        /// <param name="lvl">Level of unit killed</param>
        /// <param name="count">Number of enemies killed</param>
        /// <returns></returns>
        public static int GetUnitKilledAttackPoint(ushort type, byte lvl, int count) {
            return UnitFactory.GetUnitStats(type, lvl).Upkeep * count;
        }
        
        /// <summary>
        /// Gets the ammount of attack points the attacker should receive for killing a structure.
        /// </summary>
        /// <param name="type">Type of structure destroyed</param>
        /// <param name="lvl">Level of structure destroyed</param>
        /// <returns></returns>
        public static int GetStructureKilledAttackPoint(ushort type, byte lvl) {
            return lvl * 20;
        }
    }
}