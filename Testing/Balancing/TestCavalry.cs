#region

using ConsoleSimulator;
using Xunit;

#endregion

namespace Testing.Balancing
{
    /// <summary>
    ///     Summary description for TroopProcedureTest
    /// </summary>
    public class TestCavalry
    {
        [Fact(Skip = "For Anthony only")]
        public void TestDef10Swordsman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Swordsman, 5, -.2);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestDef10Archer()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Archer, 5, 0);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestDef10Pikeman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Pikeman, 5, -.1);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestDef10Gladiator()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Gladiator, 5, -.2);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestDef10Cavalry()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Cavalry, 2.5, -.4);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestDef10Knight()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Pikeman, 2.5, -.6);
        }

        //*******************  Attacking *********************/
        [Fact(Skip = "For Anthony only")]
        public void TestAtk10Swordsman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Cavalry, 5, -.6);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestAtk10Archer()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Archer, UnitType.Cavalry, 5, -.4);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestAtk10Pikeman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Pikeman, UnitType.Cavalry, 5, -.2);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestAtk10Gladiator()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Gladiator, UnitType.Cavalry, 5, -.4);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestAtk10Cavalry()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Cavalry, 2.5, -.2);
        }

        [Fact(Skip = "For Anthony only")]
        public void TestAtk10Knight()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Pikeman, UnitType.Cavalry, 2.5, .4);
        }
    }
}