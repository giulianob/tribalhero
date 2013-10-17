#region

using Game.Data.Troop;
using Game.Logic.Procedures;
using Ploeh.AutoFixture;
using Xunit;

#endregion

namespace Testing.TroopTests
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    public class TroopProcedureTest
    {
        private readonly ITroopStub stub;

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
            stub.AddUnit(FormationType.Normal, 101, 10);

            var fixture = new Fixture();
            var procedure = fixture.Create<CityBattleProcedure>();

            procedure.MoveUnitFormation(stub, FormationType.Normal, FormationType.InBattle);
            procedure.MoveUnitFormation(stub, FormationType.InBattle, FormationType.Normal);

            Assert.True(stub[FormationType.Normal].Type == FormationType.Normal);
            Assert.True(stub[FormationType.InBattle].Type == FormationType.InBattle);
        }
    }
}