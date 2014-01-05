#region

using System.Linq;
using Game.Data;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        ///     Returns the maximum number of forests the lumbermill can be harvesting from at one time.
        public virtual int GetMaxForestCapacity()
        {
            return 400;
        }
        ///     Returns the maximum labors allowed in the forest
        /// <param name="level"></param>
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