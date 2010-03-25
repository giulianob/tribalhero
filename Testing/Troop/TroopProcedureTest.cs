using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Troop {
    /// <summary>
    /// Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class TroopProcedureTest {

        TroopStub stub;

        [TestInitialize]
        public void TestInitialize() {            
            stub = new TroopStub();
            stub.AddFormation(FormationType.NORMAL);
            stub.AddFormation(FormationType.GARRISON);
            stub.AddFormation(FormationType.IN_BATTLE);
        }
        
        [TestCleanup]
        public void TestCleanup() { }

        [TestMethod]
        public void TestMoveFromBattleToNormal() {
            stub.AddUnit(FormationType.NORMAL, 101, 10);

            Procedure.MoveUnitFormation(stub, FormationType.NORMAL, FormationType.IN_BATTLE);
            Procedure.MoveUnitFormation(stub, FormationType.IN_BATTLE, FormationType.NORMAL);

            Assert.IsTrue(stub[FormationType.NORMAL].Type == FormationType.NORMAL);
            Assert.IsTrue(stub[FormationType.IN_BATTLE].Type == FormationType.IN_BATTLE);

            
        }
    }
}
