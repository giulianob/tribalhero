#region

using Game.Data;
using Game.Data.Forest;
using Game.Setup;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {

        /// <summary>
        ///     Returns the maximum labors allowed in the forest
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public virtual ushort GetForestMaxLabor(byte level)
        {
            return (ushort)(level * 320);
        }

        /// <summary>
        ///     Returns the maximum forest level the lumbermill is allowed to harvest from.
        /// </summary>
        /// <param name="level">Level of the lumbermill</param>
        /// <returns></returns>
        public virtual byte GetMaxForestLevel(byte level)
        {
            if (level <= 2)
            {
                return 1;
            }

            if (level <= 6)
            {
                return 2;
            }

            if (level <= 11)
            {
                return 3;
            }

            return 4;
        }

        /// <summary>
        ///     Gets the maximum capacity of the forest
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public virtual int GetMaxForestCapacity()
        {
            return 400;
        }

        public virtual int GetForestUpkeep(int count)
        {
            return count > 0 ? 2 + count : 0;
        }
    }
}