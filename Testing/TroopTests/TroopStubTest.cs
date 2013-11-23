using Common.Testing;
using FluentAssertions;
using Game.Data.Troop;
using Game.Data;
using Xunit;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.TroopTests
{
    public class TroopStubTest
    {

        public TroopStubTest()
        {
            Global.Current = Substitute.For<IGlobal>();
            Global.Current.FireEvents.Returns(false);
        }

        public void Dispose()
        {
            Global.Current = null;
        }

        [Theory, AutoNSubstituteData]
        public void TestUnitListsNoUnitsInStub(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            var units = stub.ToUnitList();

            units.Count.Should().Be(0);
        }

        [Theory, AutoNSubstituteData]
        public void TestUnitListNoConflictingTypes(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Normal, 2, 5);

            var units = stub.ToUnitList();

            Assert.True(units.Count == 2);
            Assert.True(units[0].Type == 1);
            Assert.True(units[0].Count == 4);
            Assert.True(units[1].Type == 2);
            Assert.True(units[1].Count == 5);
        }

        [Theory, AutoNSubstituteData]
        public void TestUnitListConflictingTypes(TroopStub stub)
        {            
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Attack, 1, 5);

            var units = stub.ToUnitList();

            Assert.True(units.Count == 1);
            Assert.True(units[0].Type == 1);
            Assert.True(units[0].Count == 9);
        }

        [Theory, AutoNSubstituteData]
        public void TestUnitListConflictingAndNonConflictingTypes(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Normal, 1, 5);
            stub.AddUnit(FormationType.Normal, 2, 10);

            var units = stub.ToUnitList();

            Assert.True(units.Count == 2);
            Assert.True(units[0].Type == 1);
            Assert.True(units[0].Count == 9);
            Assert.True(units[1].Type == 2);
            Assert.True(units[1].Count == 10);
        }

        [Theory, AutoNSubstituteData]
        public void TestUnitListSpecificFormations(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Attack, 1, 5);
            stub.AddUnit(FormationType.Attack, 2, 10);

            var units = stub.ToUnitList(FormationType.Attack);

            Assert.True(units.Count == 2);
            Assert.True(units[0].Type == 1);
            Assert.True(units[0].Count == 5);
            Assert.True(units[1].Type == 2);
            Assert.True(units[1].Count == 10);
        }
    }
}