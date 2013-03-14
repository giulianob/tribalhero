#region

using Game.Data.Troop;
using Game.Logic.Procedures;
using Moq;
using Persistance;
using Xunit;

#endregion

namespace Testing.Troop
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    public class TroopProcedureTest
    {
        private ITroopStub stub;

        public TroopProcedureTest()
        {
            stub = new TroopStub(0,null);
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Garrison);
            stub.AddFormation(FormationType.InBattle);
        }

        [Fact]
        public void TestMoveFromBattleToNormal()
        {
            Mock<IDbManager> dbManager = new Mock<IDbManager>();

            stub.AddUnit(FormationType.Normal, 101, 10);

            new Procedure(dbManager.Object).MoveUnitFormation(stub, FormationType.Normal, FormationType.InBattle);
            new Procedure(dbManager.Object).MoveUnitFormation(stub, FormationType.InBattle, FormationType.Normal);

            Assert.True(stub[FormationType.Normal].Type == FormationType.Normal);
            Assert.True(stub[FormationType.InBattle].Type == FormationType.InBattle);
        }
    }
}