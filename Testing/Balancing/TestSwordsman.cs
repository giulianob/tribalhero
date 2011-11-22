#region

using ConsoleSimulator;
using Xunit;

#endregion

namespace Testing.Balancing
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    public class TestSwordsman {
        [Fact]
        public void TestDefSwordsman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Swordsman, 10, -0.3);
        }
        [Fact]
        public void TestDefArcher() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Archer, 10, .3);
        }
        [Fact]
        public void TestDefPikeman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, .3);
        }
        [Fact]
        public void TestDefGladiator() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Gladiator, 10, -.1);
        }
        [Fact]
        public void TestDefCavalry() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Cavalry, 5, -0.7);
        }
        [Fact]
        public void Test10SwordsmanDef10Knight() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Knight, 10, -0.4);
        }
    }
}