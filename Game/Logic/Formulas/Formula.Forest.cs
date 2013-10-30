#region

using System.Linq;
using Game.Data;
using Game.Data.Forest;
using Game.Setup;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
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

        /// <summary>
        ///      Gets the depletion rate of the forest
        /// </summary>
        /// <param name="count">number of camps in the forest</param>
        /// <returns></returns>
        public virtual int GetForestUpkeep(int count)
        {
            return count > 0 ? 2 + count : 0;
        }

        public virtual int GetLumbermillMaxLabor(IStructure lumbermill)
        {
            int[] maxLabor = { 0, 40, 40, 80, 160, 160, 160, 240, 240, 360, 360, 360, 480, 480, 480, 640 };
            return maxLabor[lumbermill.Lvl];
        }

        public virtual string GetForestCampLaborerString(IStructure lumbermill)
        {
            int max = GetLumbermillMaxLabor(lumbermill);
            int cur = lumbermill.City.Where(s => ObjectTypeFactory.IsStructureType("ForestCamp", s)).Sum(x => x.Stats.Labor);
            return string.Format("{0}/{1}", cur, max);
        }

        public virtual int GetForestCampMaxLabor(IStructure lumbermill)
        {
            int[] maxLabor = {0, 40, 40, 40, 80, 80, 80, 80, 80, 120, 120, 120, 120, 120, 240, 320};
            return maxLabor[lumbermill.Lvl];
        }

    }
}