#region

using Game.Data.Troop;
using Game.Logic.Procedures;
using Xunit;

#endregion

namespace Testing.Troop
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    public class TroopProcedureTest : TestBase
    {
        private TroopStub stub;

        public TroopProcedureTest()
        {
            stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Garrison);
            stub.AddFormation(FormationType.InBattle);
        }

        [Fact]
        public void TestMoveFromBattleToNormal()
        {
            stub.AddUnit(FormationType.Normal, 101, 10);

            Procedure.Current.MoveUnitFormation(stub, FormationType.Normal, FormationType.InBattle);
            Procedure.Current.MoveUnitFormation(stub, FormationType.InBattle, FormationType.Normal);

            Assert.True(stub[FormationType.Normal].Type == FormationType.Normal);
            Assert.True(stub[FormationType.InBattle].Type == FormationType.InBattle);
        }
    }
}