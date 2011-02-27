#region

using Game.Data;
using Game.Data.Troop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Testing.Troop
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class TroopStarveTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Global.FireEvents = false;
            ;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Global.FireEvents = true;
        }

        public TroopStub CreateSimpleStub()
        {
            var stub = new TroopStub();
            stub.AddFormation(FormationType.Normal);
            return stub;
        }

        [TestMethod]
        public void TestStarveSingleUnit()
        {
            TroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.Normal, 0, 10);

            stub.Starve();
            Assert.AreEqual(stub[FormationType.Normal][0], 9);
        }

        [TestMethod]
        public void TestStarveMultiUnit()
        {
            TroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.Normal, 0, 10);
            stub.AddUnit(FormationType.Normal, 1, 100);

            stub.Starve();
            Assert.AreEqual(stub[FormationType.Normal][0], 9);
            Assert.AreEqual(stub[FormationType.Normal][1], 95);
        }

        [TestMethod]
        public void TestStarveMultiFormation()
        {
            TroopStub stub = CreateSimpleStub();
            stub.AddFormation(FormationType.Attack);
            stub.AddUnit(FormationType.Normal, 0, 10);
            stub.AddUnit(FormationType.Attack, 0, 100);

            stub.Starve();
            Assert.AreEqual(stub[FormationType.Normal][0], 9);
            Assert.AreEqual(stub[FormationType.Attack][0], 95);
        }

        [TestMethod]
        public void TestStarveToZero()
        {
            TroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.Normal, 0, 1);

            stub.Starve();
            Assert.IsFalse(stub[FormationType.Normal].ContainsKey(0));
        }
    }
}