using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Troop {
    /// <summary>
    /// Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class TroopStarveTest {        

        [TestInitialize]
        public void TestInitialize() {
            Global.PauseEvents();
        }

        [TestCleanup]
        public void TestCleanup() {
            Global.ResumeEvents();
        }

        public TroopStub CreateSimpleStub() {
            TroopStub stub = new TroopStub();
            stub.AddFormation(FormationType.NORMAL);
            return stub;
        }

        [TestMethod]
        public void TestStarveSingleUnit() {
            TroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.NORMAL, 0, 10);

            stub.Starve();
            Assert.AreEqual(stub[FormationType.NORMAL][0], 9);
        }

        [TestMethod]
        public void TestStarveMultiUnit() {
            TroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.NORMAL, 0, 10);
            stub.AddUnit(FormationType.NORMAL, 1, 100);

            stub.Starve();
            Assert.AreEqual(stub[FormationType.NORMAL][0], 9);
            Assert.AreEqual(stub[FormationType.NORMAL][1], 95);
        }

        [TestMethod]
        public void TestStarveMultiFormation() {
            TroopStub stub = CreateSimpleStub();
            stub.AddFormation(FormationType.ATTACK);
            stub.AddUnit(FormationType.NORMAL, 0, 10);
            stub.AddUnit(FormationType.ATTACK, 0, 100);

            stub.Starve();
            Assert.AreEqual(stub[FormationType.NORMAL][0], 9);
            Assert.AreEqual(stub[FormationType.ATTACK][0], 95);
        }

        [TestMethod]
        public void TestStarveToZero() {
            TroopStub stub = CreateSimpleStub();            
            stub.AddUnit(FormationType.NORMAL, 0, 1);

            stub.Starve();
            Assert.IsFalse(stub[FormationType.NORMAL].ContainsKey(0));
        }
    }
}
