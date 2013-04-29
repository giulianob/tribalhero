using FluentAssertions;
using Game.Logic.Formulas;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class BarbarianTribeFormulaTest
    {
        [Theory]
        [InlineData(1, 1, 10)]
        [InlineData(2, 1, 20)]
        [InlineData(3, 1, 41)]
        [InlineData(4, 2, 73)]
        [InlineData(5, 2, 117)]
        [InlineData(6, 3, 171)]
        [InlineData(7, 3, 237)]
        [InlineData(8, 4, 313)]
        [InlineData(9, 5, 401)]
        [InlineData(10, 6, 500)]
        public void TestUpkeepAndUnitLevel(int level, int expectedUnitLevel, int expectedUpkeep)
        {
            var formula = Substitute.For<Formula>();

            byte unitLevel;
            int upkeep;
            formula.BarbarianTribeUpkeep((byte)level, out upkeep, out unitLevel);

            upkeep.Should().Be(expectedUpkeep);
            unitLevel.Should().Be((byte)expectedUnitLevel);
        }
    }
}
