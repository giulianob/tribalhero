using Game.Data;
using Game.Setup;

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        public void StrongholdUpkeep(byte level, out int upkeep, out byte unitLevel)
        {
            int[] sUnitLevel = new[] {1, 1, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10};

            upkeep = Config.stronghold_npc_base_upkeep + level * Config.stronghold_npc_per_lvl_upkeep;
            unitLevel = (byte)sUnitLevel[level];
        }
    }
}
