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
    public class TestSwordsman {

        [TestInitialize]
        public void TestInitialize()
        {
            Global.FireEvents = false;
            Factory.InitAll();            
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestDefSwordsman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Swordsman, 10, -0.3);
        }
        [TestMethod]
        public void TestDefArcher() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Archer, 10, .3);
        }
        [TestMethod]
        public void TestDefPikeman() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Pikeman, 10, .3);
        }
        [TestMethod]
        public void TestDefGladiator() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Gladiator, 10, -.1);
        }
        [TestMethod]
        public void TestDefCavalry() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Cavalry, 5, -0.7);
        }
        [TestMethod]
        public void Test10SwordsmanDef10Knight() {
            TestUtility.TestUnitPerUpkeep(UnitType.Swordsman, UnitType.Knight, 10, -0.4);
        }
    }
}