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
    public class TestArcher
    {
        private TroopStub stub;

        [TestInitialize]
        public void TestInitialize()
        {
            Global.FireEvents = false;
            Factory.InitAll();
            Global.DbManager.Pause();

            stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Garrison);
            stub.AddFormation(FormationType.InBattle);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void Test10AcherDef10Swordsman()
        {

            Group defender = new Group();
            defender.AddToLocal(UnitType.Archer, 1, 10);

            Group attacker = new Group();
            attacker.AddToAttack(UnitType.Swordsman, 1, 10);

            Simulation sim = new Simulation(attacker,defender);
            sim.RunTill(20);

            Assert.IsTrue(attacker.Upkeep() < 5,"swordsman[{0}] is more than 5",attacker.Upkeep());
            Assert.IsTrue(defender.Upkeep() > 5, "archer[{0}] is less than 5", defender.Upkeep());
        }
    }
}