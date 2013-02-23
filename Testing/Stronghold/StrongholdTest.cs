using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Game.Data;
using Game.Data.Stronghold;
using Game.Util;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Xunit;
using Xunit.Extensions;

namespace Testing.Stronghold
{
    public class StrongholdTest : IDisposable
    {
        public StrongholdTest()
        {
            Global.FireEvents = false;
            SystemClock.SetClock(DateTime.UtcNow);
        }

        public static IEnumerable<object[]> VictoryPointItems
        {
            get
            {
                // no bonus
                yield return new object[] {(byte)1, 5m, 0m, 3m};
                // with bonus
                yield return new object[] { (byte)5, 5m, 2.5m, 7.5m };
                // level 20
                yield return new object[] { (byte)20, 5m, 2.5m, 18.75m };
            }
        }

        [Theory, PropertyData("VictoryPointItems")]
        public void TestVictoryPoint(byte level, decimal occupiedDays, decimal bonusDays, decimal expectedValue)
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var stronghold = fixture.CreateAnonymous<Game.Data.Stronghold.Stronghold>();

            stronghold.StrongholdState = StrongholdState.Occupied;
            stronghold.Lvl = level;
            stronghold.BonusDays = bonusDays;
            stronghold.DateOccupied = SystemClock.Now.Subtract(TimeSpan.FromDays((double)occupiedDays));
            stronghold.VictoryPointRate.Should().Be(expectedValue);

        }

        [Fact]
        public void TestNonOccupiedVictoryPoint()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var stronghold = fixture.CreateAnonymous<Game.Data.Stronghold.Stronghold>();

            stronghold.StrongholdState = StrongholdState.Neutral;
            stronghold.Lvl = 5;
            stronghold.BonusDays = 10;
            stronghold.DateOccupied = SystemClock.Now.AddDays(10);
            stronghold.VictoryPointRate.Should().Be(0);

        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            SystemClock.ResyncClock();
            Global.FireEvents = true;
        }

        #endregion
    }
}
