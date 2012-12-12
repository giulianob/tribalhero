#region

using ConsoleSimulator;
using Xunit;

#endregion

namespace Testing.Balancing
{
    /// <summary>
    ///     Summary description for TroopProcedureTest
    /// </summary>
    public class TestArcher
    {
        [Fact(Skip = "For Anthony only")]
        public void Test10AcherDef10Swordsman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Archer, UnitType.Fighter, 10, .4);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test10SwordsmanDef10Swordsman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Swordsman, 10, -0.2);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test10SwordsmanDef10Archer()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Archer, 10, .3);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test10SwordsmanDef10Pikeman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, .5);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test10SwordsmanDef10Gladiator()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Gladiator, 10, -0.3);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test10SwordsmanDef10Cavalry()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, -0.7);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test10SwordsmanDef10Knight()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, -0.4);
        }
    }
}