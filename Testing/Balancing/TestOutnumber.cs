#region

using System;
using ConsoleSimulator;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game.Battle;

#endregion

namespace Testing.Troop
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class TestOutnumber
    {
        private TroopStub stub;

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
        public void Test100Vs10()
        {
            Group defender = new Group();
            defender.AddToLocal(UnitType.Swordsman, 1, 100);

            Group attacker = new Group();
            attacker.AddToAttack(UnitType.Swordsman, 1, 10);

            Simulation sim = new Simulation(attacker,defender);
            sim.RunTill(20);

            Assert.AreNotEqual(sim.CurrentRound,0);
            Assert.IsTrue(attacker.Upkeep() == 0, "swordsman[{0}] is not 0", attacker.Upkeep());
            Assert.IsTrue(90 < defender.Upkeep() && defender.Upkeep() < 96, "archer[{0}] is less than 95", defender.Upkeep());
        }

        [TestMethod]
        public void Test500Vs50() {
            Group defender = new Group();
            defender.AddToLocal(UnitType.Swordsman, 1, 500);

            Group attacker = new Group();
            attacker.AddToAttack(UnitType.Swordsman, 1, 50);

            Simulation sim = new Simulation(attacker, defender);
            sim.RunTill(20);

            Assert.AreNotEqual(sim.CurrentRound, 0);
            Assert.IsTrue(attacker.Upkeep() == 0, "swordsman[{0}] is not 0", attacker.Upkeep());
            Assert.IsTrue(450 < defender.Upkeep() && defender.Upkeep() < 475, "archer[{0}] is less than 95", defender.Upkeep());
        }

        [TestMethod]
        public void Test500Vs250() {
            Group defender = new Group();
            defender.AddToLocal(UnitType.Swordsman, 1, 500);

            Group attacker = new Group();
            attacker.AddToAttack(UnitType.Swordsman, 1, 250);

            Simulation sim = new Simulation(attacker, defender);
            sim.RunTill(20);

            Assert.IsTrue(attacker.Upkeep() == 0, "swordsman[{0}] is not 0", attacker.Upkeep());
            Assert.IsTrue(450 < defender.Upkeep() && defender.Upkeep() < 475, "archer[{0}] is less than 95", defender.Upkeep());
        }
    }
}