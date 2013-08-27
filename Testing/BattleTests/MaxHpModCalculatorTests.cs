using Common.Testing;
using FluentAssertions;
using Game.Battle.StatsModCalculator;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class MaxHpModCalculatorTests
    {
        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereAreNoMods_ShouldReturnBaseValue(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.GetResult().Should().Be(baseValue);
        }        
        
        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereIsAPercentBonusMod_ShouldReturnBaseValueTimesPercentBonus(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("PERCENT_BONUS", 50);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 0.5);
        }
        
        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereAreMultiplePercentBonusMod_ShouldReturnBaseValueTimesSumOfPercentBonus(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("PERCENT_BONUS", 20);
            maxHpModCalculator.AddMod("PERCENT_BONUS", 30);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 0.5);
        }

        [Theory, AutoNSubstituteData]
        public void GetResult_WhenTherePercentBonusModExceeds140_ShouldCapAt140(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("PERCENT_BONUS", 141);

            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.4);
        }


    }
}