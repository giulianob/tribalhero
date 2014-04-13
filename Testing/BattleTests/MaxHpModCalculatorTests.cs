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
            maxHpModCalculator.AddMod("PERCENT_BONUS", 20);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.2);
        }
        
        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereAreMultiplePercentBonusMod_ShouldReturnBaseValueTimesSumOfPercentBonus(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("PERCENT_BONUS", 10);
            maxHpModCalculator.AddMod("PERCENT_BONUS", 25);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.35);
        }

        [Theory, AutoNSubstituteData]
        public void GetResult_WhenTherePercentBonusModExceeds140_ShouldCapAt140(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("PERCENT_BONUS", 41);

            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.4);
        }

        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereIsABowShieldBonusMod_ShouldReturnBaseValueTimesBowShieldBonus(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("BOW_SHIELD_BONUS", 20);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.2);
        }

        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereIsABowShieldBonusModAndPercentBonus_ShouldReturnBaseValueTimesBowShieldBonusPlusPercentBonus(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("PERCENT_BONUS", 15); 
            maxHpModCalculator.AddMod("BOW_SHIELD_BONUS", 20);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.35);
        }

        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereAreMultipleBowShieldBonusMod_ShouldReturnBaseValueTimesMaxBowShieldBonus(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("BOW_SHIELD_BONUS", 10);
            maxHpModCalculator.AddMod("BOW_SHIELD_BONUS", 20);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.2);
        }

        [Theory, AutoNSubstituteData]
        public void GetResult_WhenThereIsABowShieldBonusModAndOverLimitPercentBonus_ShouldReturnBaseValueTimesBowShieldBonusPlusMaxPercentBonus(
            [Frozen] double baseValue,
            MaxHpModCalculator maxHpModCalculator)
        {
            maxHpModCalculator.AddMod("PERCENT_BONUS", 41);
            maxHpModCalculator.AddMod("BOW_SHIELD_BONUS", 20);
            maxHpModCalculator.GetResult().Should().Be(baseValue * 1.6);
        }
    }
}