using Game.Data.Troop;
using Xunit;

namespace Testing.Troop
{
    public class TroopStubTest
    {
        [Fact]
        public void TestUnitListsNoUnitsInStub()
        {
            ITroopStub stub = new TroopStub(0, null);
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            var units = stub.ToUnitList();

            Assert.True(units.Count == 0);
        }

        [Fact]
        public void TestUnitListNoConflictingTypes()
        {
            ITroopStub stub = new TroopStub(0, null);
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

        [Fact]
        public void TestUnitListConflictingTypes()
        {
            ITroopStub stub = new TroopStub(0, null);
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Attack, 1, 5);

            var units = stub.ToUnitList();

            Assert.True(units.Count == 1);
            Assert.True(units[0].Type == 1);
            Assert.True(units[0].Count == 9);
        }

        [Fact]
        public void TestUnitListConflictingAndNonConflictingTypes()
        {
            ITroopStub stub = new TroopStub(0, null);
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

        [Fact]
        public void TestUnitListSpecificFormations()
        {
            ITroopStub stub = new TroopStub(0, null);
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