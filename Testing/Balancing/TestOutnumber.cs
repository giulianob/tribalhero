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
        const double ERROR_MARGIN = .0;

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

        private void TestMultiplier(int baseCount, double multiplier, double expectAdvantage)
        {
            Group defender = new Group();
            defender.AddToLocal(UnitType.TestSwordsman, 1, (ushort)(baseCount * multiplier));
            Group attacker = new Group();
            attacker.AddToAttack(UnitType.TestSwordsman, 1, (ushort)baseCount);
            Simulation sim = new Simulation(attacker,defender);
            sim.Run();

            double actualAdvantage = defender.Upkeep() * multiplier / baseCount;
            // Assert.IsTrue(attacker.Upkeep() == 0, "swordsman[{0}] is not 0", attacker.Upkeep());
            Assert.IsTrue(expectAdvantage - ERROR_MARGIN < actualAdvantage && actualAdvantage < expectAdvantage + ERROR_MARGIN, "Multi[{0}] Base[{3}] Left[{4}]'s actual advantage[{1}] is close to [{2}] expect left[{5}]", multiplier, actualAdvantage, expectAdvantage, baseCount * multiplier, defender.Upkeep(), multiplier * baseCount * expectAdvantage);
            //Assert.IsTrue(expectLeft - ERROR_MARGIN * BASE_COUNT < defender.Upkeep() && defender.Upkeep() < expectLeft + ERROR_MARGIN* BASE_COUNT, "Multi[{0}] Base[{3}] Left[{4}]'s actual advantage[{1}] is close to [{2}] expect left[{5}]", multiplier, actualAdvantage, expectAdvantage, BASE_COUNT * multiplier, defender.Upkeep(), expectLeft);
        }

        [TestMethod]
        public void Test100X100() {
            TestMultiplier(100, 0, 0);
        }
        [TestMethod]
        public void Test100X125() {
            TestMultiplier(100, 1.25, .24);
        }
        [TestMethod]
        public void Test100X150() {
            TestMultiplier(100, 1.5, .47);
        }
        [TestMethod]
        public void Test100X200() {
            TestMultiplier(100, 2, .75);
        }
        [TestMethod]
        public void Test100X350() {
            TestMultiplier(100, 3.5, .85);
        }
        [TestMethod]
        public void Test100X500() {
            TestMultiplier(100, 5, .90);
        }

        [TestMethod]
        public void Test500X100()
        {
            TestMultiplier(500, 0, 0);
        }
        [TestMethod]
        public void Test500X125() {
            TestMultiplier(500, 1.25, .24);
        }
        [TestMethod]
        public void Test500X150() {
            TestMultiplier(500, 1.5, .47);
        }
        [TestMethod]
        public void Test500X200() {
            TestMultiplier(500, 2, .75);
        }
        [TestMethod]
        public void Test500X350() {
            TestMultiplier(500, 3.5, .85);
        }
        [TestMethod]
        public void Test500X500() {
            TestMultiplier(500, 5, .90);
        }

        [TestMethod]
        public void Test1000X100() {
            TestMultiplier(1000, 0, 0);
        }
        [TestMethod]
        public void Test1000X125() {
            TestMultiplier(1000, 1.25, .24);
        }
        [TestMethod]
        public void Test1000X150() {
            TestMultiplier(1000, 1.5, .47);
        }
        [TestMethod]
        public void Test1000X200() {
            TestMultiplier(1000, 2, .75);
        }
        [TestMethod]
        public void Test1000X350() {
            TestMultiplier(1000, 3.5, .85);
        }
        [TestMethod]
        public void Test1000X500() {
            TestMultiplier(1000, 5, .90);
        }

    }
}