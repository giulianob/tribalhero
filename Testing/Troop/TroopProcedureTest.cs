#region

using Game.Data.Troop;
using Game.Logic.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Testing.Troop
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class TroopProcedureTest : TestBase
    {
        private TroopStub stub;

        [TestInitialize]
        public void TestInitialize()
        {
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
        public void TestMoveFromBattleToNormal()
        {
            stub.AddUnit(FormationType.Normal, 101, 10);

            Procedure.MoveUnitFormation(stub, FormationType.Normal, FormationType.InBattle);
            Procedure.MoveUnitFormation(stub, FormationType.InBattle, FormationType.Normal);

            Assert.IsTrue(stub[FormationType.Normal].Type == FormationType.Normal);
            Assert.IsTrue(stub[FormationType.InBattle].Type == FormationType.InBattle);
        }
    }
}