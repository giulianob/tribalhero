using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Formulas;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class TimeTest
    {
        [Theory]
        [InlineAutoNSubstituteData(100, 0, 100)]
        [InlineAutoNSubstituteData(100, 1, 100)]
        [InlineAutoNSubstituteData(100, 2, 100)]
        [InlineAutoNSubstituteData(100, 3, 100)]
        [InlineAutoNSubstituteData(100, 4, 100)]
        [InlineAutoNSubstituteData(100, 5, 100)]
        [InlineAutoNSubstituteData(100, 6, 100)]
        [InlineAutoNSubstituteData(100, 7, 100)]
        [InlineAutoNSubstituteData(100, 8, 100)]
        [InlineAutoNSubstituteData(100, 9, 95)]
        [InlineAutoNSubstituteData(100, 10, 90)]
        [InlineAutoNSubstituteData(100, 11, 85)]
        [InlineAutoNSubstituteData(100, 12, 85)]
        [InlineAutoNSubstituteData(100, 13, 80)]
        [InlineAutoNSubstituteData(100, 14, 70)]
        [InlineAutoNSubstituteData(100, 15, 60)]
        [InlineAutoNSubstituteData(100, 99, 60)]
        public void TrainTime_WhenHasDiscount_ShouldLowerTime(int baseTime,
                                                              int structureLevel,
                                                              int expected,
                                                              ITechnologyManager techManager,
                                                              ICity city,
                                                              IBaseUnitStats baseUnitStats,
                                                              Formula formula)
        {
            techManager.GetEffects(0).ReturnsForAnyArgs(new List<Effect>());
            baseUnitStats.BuildTime.Returns(baseTime);

            formula.TrainTime(structureLevel, 2, baseUnitStats).Should().Be(expected*2);
        }

        [Theory, AutoNSubstituteData]
        public void TrainTime_WhenNoDiscount_ShouldNotLowerTime(int baseTime,
                                                                int expected,
                                                                ITechnologyManager techManager,
                                                                IBaseUnitStats baseUnitStats,
                                                                ICity city,
                                                                Formula formula)
        {
            city.Troops.Upkeep.Returns(15);
            baseUnitStats.BuildTime.Returns(100);

            // Act
            formula.TrainTime(1, 1, baseUnitStats).Should().Be(100);
        }

    }
}