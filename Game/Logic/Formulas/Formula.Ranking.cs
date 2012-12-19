#region

using System;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        /// <summary>
        ///     Gets the ammount of attack points that should be received
        /// </summary>
        /// <param name="enemiesKilled">Value of enemy units killed</param>
        /// <returns></returns>
        public virtual int GetAttackPoint(int enemiesKilled)
        {
            return Math.Max(0, enemiesKilled);
        }

        /// <summary>
        ///     Gets the ammount of attack points the attacker should receive for killing an enemy.
        /// </summary>
        /// <param name="type">Type of unit killed</param>
        /// <param name="lvl">Level of unit killed</param>
        /// <param name="count">Number of enemies killed</param>
        /// <returns></returns>
        public virtual int GetUnitKilledAttackPoint(ushort type, byte lvl, int count)
        {
            return UnitFactory.GetUnitStats(type, lvl).Upkeep * count;
        }

        /// <summary>
        ///     Gets the ammount of attack points the attacker should receive for killing a structure.
        /// </summary>
        /// <param name="type">Type of structure destroyed</param>
        /// <param name="lvl">Level of structure destroyed</param>
        /// <returns></returns>
        public virtual int GetStructureKilledAttackPoint(ushort type, byte lvl)
        {
            return lvl * 20;
        }
    }
}