#region

using Game.Data.Troop;
using Xunit;

#endregion

namespace Testing.TroopTests
{
    /// <summary>
    ///     Summary description for TroopProcedureTest
    /// </summary>
    public class TroopStarveTest
    {
        public ITroopStub CreateSimpleStub()
        {
            var stub = new TroopStub(0, null);
            stub.AddFormation(FormationType.Normal);
            return stub;
        }

        [Fact]
        public void TestStarveSingleUnit()
        {
            ITroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.Normal, 0, 10);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 9);
        }

        [Fact]
        public void TestStarveMultiUnit()
        {
            ITroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.Normal, 0, 10);
            stub.AddUnit(FormationType.Normal, 1, 100);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 9);
            Assert.Equal(stub[FormationType.Normal][1], 95);
        }

        [Fact]
        public void TestStarveMultiFormation()
        {
            ITroopStub stub = CreateSimpleStub();
            stub.AddFormation(FormationType.Attack);
            stub.AddUnit(FormationType.Normal, 0, 10);
            stub.AddUnit(FormationType.Attack, 0, 100);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 9);
            Assert.Equal(stub[FormationType.Attack][0], 95);
        }

        [Fact]
        public void TestStarveToZero()
        {
            ITroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.Normal, 0, 1);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 1);
        }

        [Fact]
        public void TestStarveToZeroBypassProtection()
        {
            ITroopStub stub = CreateSimpleStub();
            stub.AddUnit(FormationType.Normal, 0, 1);

            stub.Starve(5, true);
            Assert.False(stub[FormationType.Normal].ContainsKey(0));
        }
    }
}