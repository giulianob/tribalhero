using Common.Testing;
using FluentAssertions;
using Game.Battle.StatsModCalculator;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class IntStatsModCalculatorTests
    {
        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereAreNoMods_ShouldReturnBaseValue(
            [Frozen] int baseValue,
            IntStatsModCalculator intStatsModCalculator)
        {
            intStatsModCalculator.GetResult().Should().Be(baseValue);
        }        
        
        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereIsAPercentBonusMod_ShouldReturnBaseValueTimesPercentBonus(
            [Frozen] int baseValue,
            IntStatsModCalculator intStatsModCalculator)
        {
            intStatsModCalculator.AddMod("PERCENT_BONUS", 20);
            intStatsModCalculator.GetResult().Should().Be((int)(baseValue * 1.2));
        }
        
        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereAreMultiplePercentBonusMod_ShouldReturnBaseValueTimesSumOfPercentBonus(
            [Frozen] int baseValue,
            IntStatsModCalculator intStatsModCalculator)
        {
            intStatsModCalculator.AddMod("PERCENT_BONUS", 10);
            intStatsModCalculator.AddMod("PERCENT_BONUS", 25);
            intStatsModCalculator.GetResult().Should().Be((int)(baseValue * 1.35));
        }

        [Theory, AutoNSubstituteData]
        public void GetResult_WhenTherePercentBonusModExceeds140_ShouldCapAt140(
            [Frozen] int baseValue,
            IntStatsModCalculator intStatsModCalculator)
        {
            intStatsModCalculator.AddMod("PERCENT_BONUS", 141);

            intStatsModCalculator.GetResult().Should().Be((int)(baseValue * 2.41));
        }


    }
}