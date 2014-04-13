#region

using System;
using FluentAssertions;
using Game.Data;
using Game.Setup;
using Game.Util;
using Xunit;
using Xunit.Extensions;

#endregion

namespace Testing.ResourcesTests
{
    /// <summary>
    ///     Summary description for LazyResourceTest
    /// </summary>
    public class LazyResourceTest : IDisposable
    {
        private DateTime begin = DateTime.UtcNow;

        public LazyResourceTest()
        {
            SystemClock.SetClock(begin);
            Config.seconds_per_unit = 1;
        }

        public void Dispose()
        {
            SystemClock.ResyncClock();
        }

        /// <summary>
        ///     Tests rate and upkeep being zero
        /// </summary>
        [Fact]
        public void TestZero()
        {
            var resource = new LazyValue(0, begin, 0, 0);
            SystemClock.SetClock(begin.AddHours(12));
            Assert.Equal(resource.Value, 0);
        }

        /// <summary>
        ///     Tests having positive rate but no upkeep
        /// </summary>
        [Fact]
        public void TestPositiveRate()
        {
            var resource = new LazyValue(0, begin, 100, 0);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 50);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 100);
        }

        /// <summary>
        ///     Tests having positive upkeep but no rate
        /// </summary>
        [Fact]
        public void TestPositiveUpkeep()
        {
            var resource = new LazyValue(0, begin, 0, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 0);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 0);
        }

        /// <summary>
        ///     Tests having rate higher than upkeep
        /// </summary>
        [Fact]
        public void TestPositiveRateGreaterThanUpkeep()
        {
            var resource = new LazyValue(0, begin, 100, 50);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 25);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 50);
        }

        /// <summary>
        ///     Tests having upkeep higher than rate
        /// </summary>
        [Fact]
        public void TestPositiveUpkeepGreaterThanRate()
        {
            var resource = new LazyValue(0, begin, 50, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 0);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 0);
        }

        /// <summary>
        ///     Test equal upkeep and rate
        /// </summary>
        [Fact]
        public void TestEqualUpkeepAndRate()
        {
            var resource = new LazyValue(0, begin, 100, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 0);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 0);
        }

        /// <summary>
        ///     Tests changing the rate
        /// </summary>
        [Fact]
        public void TestChangeRate()
        {
            var resource = new LazyValue(0, begin, 0, 0);

            // Set the rate higher            
            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Rate = 100;
            Assert.Equal(resource.Value, 0);

            // Probe later
            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 50);
        }

        /// <summary>
        ///     Tests changing the upkeep
        /// </summary>
        [Fact]
        public void TestChangeUpkeep()
        {
            var resource = new LazyValue(0, begin, 100, 0);

            // Set the upkeep higher
            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Upkeep = 50;
            Assert.Equal(resource.Value, 50);

            // Probe later
            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 75);
        }

        [Fact]
        public void TestAdd()
        {
            var resource = new LazyValue(0, begin, 100, 0);

            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Add(25);
            Assert.Equal(resource.Value, 75);
        }

        [Fact]
        public void TestSubtract()
        {
            var resource = new LazyValue(0, begin, 100, 0);

            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Subtract(30);
            Assert.Equal(resource.Value, 20);
        }

        [Fact]
        public void TestComprehensive1()
        {
            var resource = new LazyValue(0, begin, 0, 0);

            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Upkeep = 50;
            Assert.Equal(resource.Value, 0);

            SystemClock.SetClock(begin.AddMinutes(60));
            resource.Rate = 100;
            Assert.Equal(resource.Value, 0);

            SystemClock.SetClock(begin.AddMinutes(90));
            Assert.Equal(resource.Value, 25);

            SystemClock.SetClock(begin.AddMinutes(120));
            resource.Subtract(25);
            Assert.Equal(resource.Value, 25);

            SystemClock.SetClock(begin.AddMinutes(150));
            resource.Rate = 0;
            resource.Upkeep = 0;
            Assert.Equal(resource.Value, 50);

            SystemClock.SetClock(begin.AddMinutes(300));
            Assert.Equal(resource.Value, 50);
        }

        /// <summary>
        ///     Tests having positive rate but no upkeep and secs per unit modified
        /// </summary>
        [Fact]
        public void TestPositiveRateWithSecsPerUnit()
        {
            Config.seconds_per_unit = 0.1;

            var resource = new LazyValue(0, begin, 100, 0);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 500);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 1000);
        }

        /// <summary>
        ///     Tests having rate higher than upkeep and secs per unit
        /// </summary>
        [Fact]
        public void TestPositiveRateGreaterThanUpkeepWithSecsPerUnit()
        {
            Config.seconds_per_unit = 0.01;

            var resource = new LazyValue(0, begin, 100, 50);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 2500);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 5000);
        }

        /// <summary>
        ///     Tests get amount received with various scenarios
        /// </summary>
        [Theory,
         InlineData(3600, 0, 0, 0),
         InlineData(600, 0, 0, 0),
         InlineData(600, 10, 0, 1),
         InlineData(600, 60, 0, 10),
         InlineData(3600, 60, 0, 60),
         InlineData(3600, 60, 30, 30),
         InlineData(1800, 60, 30, 15),
         InlineData(1800, 30, 60, -15),
         InlineData(1800, 11, 60, -24),
         InlineData(3600, 30, 60, -30),
         InlineData(1800, 30, 30, 0),        
         InlineData(18, 100000, 43400, 282)]
        // No rate or upkeep for 1 hr should return 0
        // No rate or upkeep for 10 min should return 0
        // Rate of 10 for 10 min should return 1
        // Rate of 60 for 10 min should return 10
        // Rate of 60 for 1 hour should return 60
        // Rate of 60 with 30 upkeep for 1 hour should return 30
        // Rate of 60 with 30 upkeep for 30 min should return 15
        // Rate of 30 with 60 upkeep for 30 min should return -15
        // Rate of 11 with 60 upkeep for 30 min should return -24
        // Rate of 60 with 30 upkeep for 1 hour should return -30
        // Rate of 30 with 30 upkeep for 1 hour should return
        // The last value should technically be 283 but decimal is giving 282.99999999 which when casted to an int truncates
        public void TestGetAmountReceived(int interval, int rate, int upkeep, int expected)
        {
            var resource = new LazyValue(0, DateTime.MinValue, rate, upkeep);
            resource.GetAmountReceived(interval).Should().Be(expected);
        }

        [Theory, 
        InlineData(100, 100, 100),
        InlineData(100, 101, 100),
        InlineData(0, 100000, 99999)]
        public void Add_WhenResourceGoesOverCap_ShouldLimitBasedOnCap(int limit, int valueToAdd, int expected)
        {
            var lazyValue = new LazyValue(0);
            lazyValue.Limit = limit;
            lazyValue.Add(valueToAdd);
            lazyValue.Value.Should().Be(expected);
        }

        [Theory, 
        InlineData(100, 50, 100),
        InlineData(0, 99900, 99999)]
        public void Value_WhenResourceGoesOverCap_ShouldLimitBasedOnCap(int limit, int initialValue, int expected)
        {
            SystemClock.SetClock(DateTime.UtcNow);

            var lazyValue = new LazyValue(initialValue);
            lazyValue.Limit = limit;
            lazyValue.Rate = 1000;
            lazyValue.Value.Should().Be(initialValue);
            
            SystemClock.SetClock(DateTime.UtcNow.AddDays(5));
            lazyValue.Value.Should().Be(expected);
        }
    }
}