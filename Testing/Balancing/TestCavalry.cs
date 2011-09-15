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
    public class TestCavalry : TestBase
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
        public void TestDef10Swordsman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Swordsman, 5, -.2);
        }
        [TestMethod]
        public void TestDef10Archer() {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Archer, 5, 0);
        }
        [TestMethod]
        public void TestDef10Pikeman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Pikeman, 5, -.1);
        }
        [TestMethod]
        public void TestDef10Gladiator() {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Gladiator, 5, -.2);
        }
        [TestMethod]
        public void TestDef10Cavalry() {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Cavalry, 2.5, -.4);
        }
        [TestMethod]
        public void TestDef10Knight() {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Pikeman, 2.5, -.6);
        }
        //*******************  Attacking *********************/
        [TestMethod]
        public void TestAtk10Swordsman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Cavalry, 5, -.6);
        }
        [TestMethod]
        public void TestAtk10Archer() {
            TestUtility.TestUnitPerUpkeep(UnitType.Archer, UnitType.Cavalry, 5, -.4);
        }
        [TestMethod]
        public void TestAtk10Pikeman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Pikeman, UnitType.Cavalry, 5, -.2);
        }
        [TestMethod]
        public void TestAtk10Gladiator() {
            TestUtility.TestUnitPerUpkeep(UnitType.Gladiator, UnitType.Cavalry, 5, -.4);
        }
        [TestMethod]
        public void TestAtk10Cavalry() {
            TestUtility.TestUnitPerUpkeep(UnitType.Cavalry, UnitType.Cavalry, 2.5, -.2);
        }
        [TestMethod]
        public void TestAtk10Knight() {
            TestUtility.TestUnitPerUpkeep(UnitType.Pikeman, UnitType.Cavalry, 2.5, .4);
        }
    }
}