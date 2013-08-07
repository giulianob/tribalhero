using Common.Testing;
using FluentAssertions;
using Game.Logic.Formulas;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class BarbarianTribeFormulaTest
    {
        [Theory]
        [InlineAutoNSubstituteData(1, 1, 10)]
        [InlineAutoNSubstituteData(2, 1, 20)]
        [InlineAutoNSubstituteData(3, 1, 41)]
        [InlineAutoNSubstituteData(4, 2, 73)]
        [InlineAutoNSubstituteData(5, 2, 117)]
        [InlineAutoNSubstituteData(6, 3, 171)]
        [InlineAutoNSubstituteData(7, 3, 237)]
        [InlineAutoNSubstituteData(8, 4, 313)]
        [InlineAutoNSubstituteData(9, 5, 401)]
        [InlineAutoNSubstituteData(10, 6, 500)]
        public void TestUpkeepAndUnitLevel(int level, int expectedUnitLevel, int expectedUpkeep, Formula formula)
        {
            byte unitLevel;
            int upkeep;
            formula.BarbarianTribeUpkeep((byte)level, out upkeep, out unitLevel);

            upkeep.Should().Be(expectedUpkeep);
            unitLevel.Should().Be((byte)expectedUnitLevel);
        }
    }
}
