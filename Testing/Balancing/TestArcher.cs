#region

using System;
using ConsoleSimulator;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game.Battle;
using Testing.Balancing;

#endregion

namespace Testing.Troop
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class TestArcher : TestBase
    {

        [TestInitialize]
        public void TestInitialize()
        {
            Global.FireEvents = false;
            Factory.InitAll();
            Global.DbManager.Pause();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void Test10AcherDef10Swordsman()
        {
            TestUtility.TestUnitPerUpkeep(UnitType.Archer, UnitType.Fighter, 10, .4);
        }

        [TestMethod]
        public void Test10SwordsmanDef10Swordsman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Swordsman, 10, -0.2);
        }
        [TestMethod]
        public void Test10SwordsmanDef10Archer() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Archer, 10, .3);
        }
        [TestMethod]
        public void Test10SwordsmanDef10Pikeman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, .5);
        }
        [TestMethod]
        public void Test10SwordsmanDef10Gladiator() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Gladiator, 10, -0.3);
        }
        [TestMethod]
        public void Test10SwordsmanDef10Cavalry() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, -0.7);
        }
        [TestMethod]
        public void Test10SwordsmanDef10Knight() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, -0.4);
        }
    }
}