#region

using Common.Testing;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Ploeh.AutoFixture;
using Xunit;
using Xunit.Extensions;

#endregion

namespace Testing.TroopTests
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    public class TroopProcedureTest
    {        
        [Theory, AutoNSubstituteData]
        public void TestMoveFromBattleToNormal(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Garrison);
            stub.AddFormation(FormationType.InBattle);
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