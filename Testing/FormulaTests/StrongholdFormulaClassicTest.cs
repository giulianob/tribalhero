using System;
using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Data.Stronghold;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class StrongholdFormulaClassicTest : IDisposable
    {
        public StrongholdFormulaClassicTest()
        {
            SystemClock.SetClock(DateTime.UtcNow);
        }

        [Theory]
        [InlineData(1, 1000)]
        [InlineData(2, 2000)]
        [InlineData(3, 3000)]
        [InlineData(4, 4000)]
        [InlineData(5, 5000)]
        [InlineData(6, 6000)]
        [InlineData(7, 7000)]
        [InlineData(8, 8000)]
        [InlineData(9, 9000)]
        [InlineData(10, 10000)]
        [InlineData(11, 11000)]
        [InlineData(12, 12000)]
        [InlineData(13, 13000)]
        [InlineData(14, 14000)]
        [InlineData(15, 15000)]
        [InlineData(16, 16000)]
        [InlineData(17, 17000)]
        [InlineData(18, 18000)]
        [InlineData(19, 19000)]
        [InlineData(20, 20000)]
        public void TestMeterValue(int level, int expectedValue)
        {
            var formula = new Fixture().Create<Formula>();
            formula.StrongholdMainBattleMeter((byte)level).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 10000)]
        [InlineData(2, 13500)]
        [InlineData(3, 17300)]
        [InlineData(4, 21500)]
        [InlineData(5, 26200)]
        [InlineData(6, 31300)]
        [InlineData(7, 37100)]
        [InlineData(8, 43400)]
        [InlineData(9, 50300)]
        [InlineData(10, 58000)]
        [InlineData(11, 66500)]
        [InlineData(12, 75900)]
        [InlineData(13, 86300)]
        [InlineData(14, 97800)]
        [InlineData(15, 110500)]
        [InlineData(16, 124600)]
        [InlineData(17, 140100)]
        [InlineData(18, 157200)]
        [InlineData(19, 176200)]
        [InlineData(20, 200000)]
        public void TestGateValue(int level, int expectedValue)
        {
            var formula = new Fixture().Create<Formula>();
            formula.StrongholdGateLimit((byte)level).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 1, 1000)]
        [InlineData(2, 1, 1400)]
        [InlineData(3, 1, 1796)]
        [InlineData(4, 2, 2189)]
        [InlineData(5, 2, 2579)]
        [InlineData(6, 3, 2965)]
        [InlineData(7, 3, 3347)]
        [InlineData(8, 4, 3726)]
        [InlineData(9, 4, 4102)]
        [InlineData(10, 5, 4474)]
        [InlineData(11, 5, 4842)]
        [InlineData(12, 6, 5207)]
        [InlineData(13, 6, 5568)]
        [InlineData(14, 7, 5926)]
        [InlineData(15, 7, 6281)]
        [InlineData(16, 8, 6632)]
        [InlineData(17, 8, 6979)]
        [InlineData(18, 9, 7323)]
        [InlineData(19, 9, 7663)]
        [InlineData(20, 10, 8000)]
        public void TestUpkeepValue(int level, int expectedLevel, int expectedUpkeep)
        {
            byte unitLevel;
            int upkeep;
            var formula = new Fixture().Create<Formula>();
            formula.StrongholdUpkeep((byte)level, out upkeep, out unitLevel);
            unitLevel.Should().Be((byte)expectedLevel);
            upkeep.Should().Be(expectedUpkeep);
        }

        public static IEnumerable<object[]> VictoryPointItems
        {
            get
            {
                // no bonus
                yield return new object[] { (byte)1, 5m, 0m, 0m, 12.5m };
                // with bonus
                yield return new object[] { (byte)5, 5m, 2.5m, 0m, 68.75m };
                // level 20
                yield return new object[] { (byte)20, 5m, 2.5m, 0m, 275m };
                // level 20 with 18 server days
                yield return new object[] { (byte)20, 5m, 2.5m, 18m, 275m };
                // level 8 with 40.3 server days
                yield return new object[] { (byte)8, 2.5m, 5m, 40.3m, 110m };
            }
        }

        [Theory, PropertyData("VictoryPointItems")]
        public void TestVictoryPoint(byte level, decimal occupiedDays, decimal bonusDays, decimal serverDays, decimal expectedValue)
        {
            Global.SystemVariables.Clear();
            Global.SystemVariables.Add("Server.date", new SystemVariable("Server.date", SystemClock.Now.Subtract(TimeSpan.FromDays((double)serverDays))));
            var formula = new Fixture().Create<Formula>();

            var stronghold = Substitute.For<IStronghold>();
            stronghold.StrongholdState.Returns(StrongholdState.Occupied);
            stronghold.Lvl.Returns(level);
            stronghold.BonusDays.Returns(bonusDays);
            stronghold.DateOccupied.Returns(SystemClock.Now.Subtract(TimeSpan.FromDays((double)occupiedDays)));

            formula.StrongholdVictoryPoint(stronghold).Should().Be(expectedValue);
        }

        [Fact]
        public void TestNonOccupiedVictoryPoint()
        {
            var formula = new Fixture().Create<Formula>();

            var stronghold = Substitute.For<IStronghold>();
            stronghold.StrongholdState.Returns(StrongholdState.Neutral);
            stronghold.Lvl.Returns((byte)5);
            stronghold.BonusDays.Returns(10);
            stronghold.DateOccupied.Returns(SystemClock.Now.Subtract(TimeSpan.FromDays(10)));

            formula.StrongholdVictoryPoint(stronghold).Should().Be(0);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            SystemClock.ResyncClock();
        }

        #endregion
    }
}
