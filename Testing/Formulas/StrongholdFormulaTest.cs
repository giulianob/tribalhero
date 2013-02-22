using FluentAssertions;
using Game.Logic.Formulas;
using Ploeh.AutoFixture;
using Xunit.Extensions;

namespace Testing.Formulas
{
    public class StrongholdFormulaTest
    {

        [Theory]
        [InlineData(1, 500)]
        [InlineData(2, 600)]
        [InlineData(3, 700)]
        [InlineData(4, 800)]
        [InlineData(5, 950)]
        [InlineData(6, 1100)]
        [InlineData(7, 1250)]
        [InlineData(8, 1450)]
        [InlineData(9, 1650)]
        [InlineData(10, 1850)]
        [InlineData(11, 2100)]
        [InlineData(12, 2350)]
        [InlineData(13, 2600)]
        [InlineData(14, 2900)]
        [InlineData(15, 3200)]
        [InlineData(16, 3500)]
        [InlineData(17, 3850)]
        [InlineData(18, 4200)]
        [InlineData(19, 4600)]
        [InlineData(20, 5000)]
        public void TestMeterValue(int level, int expectedValue)
        {
            var formula = new Fixture().CreateAnonymous<Formula>();
            formula.StrongholdMainBattleMeter((byte)level).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 5000)]
        [InlineData(2, 6000)]
        [InlineData(3, 7000)]
        [InlineData(4, 8000)]
        [InlineData(5, 9500)]
        [InlineData(6, 11000)]
        [InlineData(7, 12500)]
        [InlineData(8, 14500)]
        [InlineData(9, 16500)]
        [InlineData(10, 18500)]
        [InlineData(11, 21000)]
        [InlineData(12, 23500)]
        [InlineData(13, 26000)]
        [InlineData(14, 29000)]
        [InlineData(15, 32000)]
        [InlineData(16, 35000)]
        [InlineData(17, 38500)]
        [InlineData(18, 42000)]
        [InlineData(19, 46000)]
        [InlineData(20, 50000)]
        public void TestGateValue(int level, int expectedValue)
        {
            var formula = new Fixture().CreateAnonymous<Formula>();
            formula.StrongholdGateLimit((byte)level).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 1, 400)]
        [InlineData(2, 1, 480)]
        [InlineData(3, 1, 560)]
        [InlineData(4, 2, 640)]
        [InlineData(5, 2, 760)]
        [InlineData(6, 3, 880)]
        [InlineData(7, 3, 1000)]
        [InlineData(8, 4, 1160)]
        [InlineData(9, 4, 1320)]
        [InlineData(10, 5, 1480)]
        [InlineData(11, 5, 1680)]
        [InlineData(12, 6, 1880)]
        [InlineData(13, 6, 2080)]
        [InlineData(14, 7, 2320)]
        [InlineData(15, 7, 2560)]
        [InlineData(16, 8, 2800)]
        [InlineData(17, 8, 3080)]
        [InlineData(18, 9, 3360)]
        [InlineData(19, 9, 3680)]
        [InlineData(20, 10, 4000)]
        public void TestUpkeepValue(int level, int expectedLevel, int expectedUpkeep)
        {
            byte unitLevel;
            int upkeep;
            var formula = new Fixture().CreateAnonymous<Formula>();
            formula.StrongholdUpkeep((byte)level, out upkeep, out unitLevel);
            unitLevel.Should().Be((byte)expectedLevel);
            upkeep.Should().Be(expectedUpkeep);
        }
    }
}
