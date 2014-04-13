using System;
using Common.Testing;
using FluentAssertions;
using Game.Logic.Formulas;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class PureVirtualTests
    {
        [Theory, AutoNSubstituteData]
        public void Formula_TestIsPureVirtual(IFormulaStub stub)
        {
            Action act = () => stub.Formula.GetInitialAp().Returns(9999);
            act.ShouldNotThrow();

            stub.Formula.GetInitialAp().Should().Be(9999);
        }

        public interface IFormulaStub
        {
            Formula Formula { get; set; }
        }
    }
}