#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {

        /// <summary>
        /// Returns the maximum number of forests the lumbermill can be harvesting from at one time.
        /// </summary>
        /// <param name="level"></param>
        internal static int GetMaxForestCount(byte level) {
            return level;
        }

        /// <summary>
        /// Returns the maximum labors allowed in the forest
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        internal static ushort GetForestMaxLabor(byte level) {
            return (ushort)(level * 300);
        }

        /// <summary>
        /// Returns the maximum forest level the lumbermill is allowed to harvest from.
        /// </summary>
        /// <param name="level">Level of the lumbermill</param>
        /// <returns></returns>
        internal static byte GetMaxForestLevel(byte level) {
            if (level <= 2)
                return 1;

            if (level <= 4)
                return 2;

            if (level <= 6)
                return 3;
            
            return 4;
        }
    }
}