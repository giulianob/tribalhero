#region

using Game.Data.Troop;
using Game.Logic.Procedures;
using Ploeh.AutoFixture;
using Xunit;

#endregion

namespace Testing.TroopTests
{
    public class BattleProcedureTest
    {
        [Fact]
        public void TestMoveFromBattleToNormal()
        {
            var fixture = new Fixture();

            var stub = new TroopStub(0, null);
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Garrison);
            stub.AddFormation(FormationType.InBattle);

            stub.AddUnit(FormationType.Normal, 101, 10);

            var cityBattleProcedure = fixture.Create<CityBattleProcedure>();

            cityBattleProcedure.MoveUnitFormation(stub, FormationType.Normal, FormationType.InBattle);
            cityBattleProcedure.MoveUnitFormation(stub, FormationType.InBattle, FormationType.Normal);

            Assert.True(stub[FormationType.Normal].Type == FormationType.Normal);
            Assert.True(stub[FormationType.InBattle].Type == FormationType.InBattle);
        }
    }
}