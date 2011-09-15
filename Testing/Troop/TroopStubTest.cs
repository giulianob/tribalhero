using Game.Data;
using Game.Data.Troop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Troop
{
    [TestClass]
    public class TroopStubTest : TestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {                        
            Global.FireEvents = false;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Global.FireEvents = true;
        }

        [TestMethod]
        public void TestUnitListsNoUnitsInStub()
        {
            TroopStub stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            var units = stub.ToUnitList();
            
            Assert.IsTrue(units.Count == 0);
        }

        [TestMethod]
        public void TestUnitListNoConflictingTypes()
        {
            TroopStub stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Normal, 2, 5);

            var units = stub.ToUnitList();

            Assert.IsTrue(units.Count == 2);
            Assert.IsTrue(units[0].Type == 1);
            Assert.IsTrue(units[0].Count == 4);
            Assert.IsTrue(units[1].Type == 2);
            Assert.IsTrue(units[1].Count == 5);
        }

        [TestMethod]
        public void TestUnitListConflictingTypes()
        {
            TroopStub stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Attack, 1, 5);

            var units = stub.ToUnitList();

            Assert.IsTrue(units.Count == 1);
            Assert.IsTrue(units[0].Type == 1);
            Assert.IsTrue(units[0].Count == 9);
        }

        [TestMethod]
        public void TestUnitListConflictingAndNonConflictingTypes()
        {
            TroopStub stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Normal, 1, 5);
            stub.AddUnit(FormationType.Normal, 2, 10);

            var units = stub.ToUnitList();

            Assert.IsTrue(units.Count == 2);
            Assert.IsTrue(units[0].Type == 1);
            Assert.IsTrue(units[0].Count == 9);
            Assert.IsTrue(units[1].Type == 2);
            Assert.IsTrue(units[1].Count == 10);
        }

        [TestMethod]
        public void TestUnitListSpecificFormations()
        {
            TroopStub stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);

            stub.AddUnit(FormationType.Normal, 1, 4);
            stub.AddUnit(FormationType.Attack, 1, 5);
            stub.AddUnit(FormationType.Attack, 2, 10);

            var units = stub.ToUnitList(FormationType.Attack);

            Assert.IsTrue(units.Count == 2);
            Assert.IsTrue(units[0].Type == 1);
            Assert.IsTrue(units[0].Count == 5);
            Assert.IsTrue(units[1].Type == 2);
            Assert.IsTrue(units[1].Count == 10);
        }
    }
}
