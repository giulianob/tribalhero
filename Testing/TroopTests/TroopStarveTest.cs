#region

using System;
using Common.Testing;
using Game.Data;
using Game.Data.Troop;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

#endregion

namespace Testing.TroopTests
{
    /// <summary>
    ///     Summary description for TroopProcedureTest
    /// </summary>
    public class TroopStarveTest : IDisposable
    {
        public TroopStarveTest()
        {
            Global.Current = Substitute.For<IGlobal>();
            Global.Current.FireEvents.Returns(false);
        }

        public void Dispose()
        {
            Global.Current = null;
        }

        [Theory, AutoNSubstituteData]
        public void TestStarveSingleUnit(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddUnit(FormationType.Normal, 0, 10);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 9);
        }

        [Theory, AutoNSubstituteData]
        public void TestStarveMultiUnit(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddUnit(FormationType.Normal, 0, 10);
            stub.AddUnit(FormationType.Normal, 1, 100);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 9);
            Assert.Equal(stub[FormationType.Normal][1], 95);
        }

        [Theory, AutoNSubstituteData]
        public void TestStarveMultiFormation(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Attack);
            stub.AddUnit(FormationType.Normal, 0, 10);
            stub.AddUnit(FormationType.Attack, 0, 100);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 9);
            Assert.Equal(stub[FormationType.Attack][0], 95);
        }

        [Theory, AutoNSubstituteData]
        public void TestStarveToZero(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddUnit(FormationType.Normal, 0, 1);

            stub.Starve();
            Assert.Equal(stub[FormationType.Normal][0], 1);
        }

        [Theory, AutoNSubstituteData]
        public void TestStarveToZeroBypassProtection(TroopStub stub)
        {
            stub.AddFormation(FormationType.Normal);
            stub.AddUnit(FormationType.Normal, 0, 1);

            stub.Starve(5, true);
            Assert.False(stub[FormationType.Normal].ContainsKey(0));
        }
    }
}