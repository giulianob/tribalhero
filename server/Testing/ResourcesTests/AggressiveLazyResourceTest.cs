#region

using System;
using Game.Data;
using Game.Util;
using Xunit;

#endregion

namespace Testing.ResourcesTests
{
    /// <summary>
    ///     Summary description for LazyResourceTest
    /// </summary>
    public class AggressiveLazyResourceTest : IDisposable
    {
        private DateTime begin = DateTime.UtcNow;

        public AggressiveLazyResourceTest()
        {
            SystemClock.SetClock(begin);
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
            var resource = new AggressiveLazyValue(0, begin, 0, 0);
            SystemClock.SetClock(begin.AddHours(12));
            Assert.Equal(resource.Value, 0);
        }

        /// <summary>
        ///     Tests having positive rate but no upkeep
        /// </summary>
        [Fact]
        public void TestPositiveRate()
        {
            var resource = new AggressiveLazyValue(0, begin, 100, 0);
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
            var resource = new AggressiveLazyValue(100, begin, 0, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(50, resource.Value);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(0, resource.Value);
        }

        /// <summary>
        ///     Tests having rate higher than upkeep
        /// </summary>
        [Fact]
        public void TestPositiveRateGreaterThanUpkeep()
        {
            var resource = new AggressiveLazyValue(0, begin, 100, 50);
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
            var resource = new AggressiveLazyValue(100, begin, 50, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 75);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 50);
        }

        /// <summary>
        ///     Test equal upkeep and rate
        /// </summary>
        [Fact]
        public void TestEqualUpkeepAndRate()
        {
            var resource = new AggressiveLazyValue(100, begin, 100, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.Equal(resource.Value, 100);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 100);
        }

        /// <summary>
        ///     Tests changing the rate
        /// </summary>
        [Fact]
        public void TestChangeRate()
        {
            var resource = new AggressiveLazyValue(0, begin, 0, 0);

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
            var resource = new AggressiveLazyValue(100, begin, 100, 0);

            // Set the upkeep higher
            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Upkeep = 50;
            resource.Rate = 0;
            Assert.Equal(resource.Value, 150);

            // Probe later
            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.Equal(resource.Value, 125);
        }
    }
}