using Game.Data;

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        public Resource BarbarianTribeResources(byte level)
        {
            return new Resource(100, 100, 100, 100);

            int[] crop = new[] {0};
            int[] wood = new[] {0};
            int[] iron = new[] {0};
            int[] gold = new[] {0};

            return new Resource(crop[level], gold[level], iron[level], wood[level]);
        }

        public void BarbarianTribeUpkeep(byte level, out int upkeep, out byte unitLevel)
        {
            int[] btUpkeep = new[] {1, 10, 20, 41, 73, 117, 171, 237, 313, 401, 500};
            int[] btUnitLevel = new[] {1, 1, 1, 1, 1, 1, 2, 3, 4, 5, 6};

            upkeep = btUpkeep[level];
            unitLevel = (byte)btUnitLevel[level];
        }

        public Resource BarbarianTribeBonus(byte level)
        {
            return new Resource();
        }
    }
}
