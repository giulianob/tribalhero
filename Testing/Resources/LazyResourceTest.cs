#region

using System;
using Game.Data;
using Game.Setup;
using Game.Util;
using Xunit;

#endregion

namespace Testing.Resources
{
    /// <summary>
    ///   Summary description for LazyResourceTest
    /// </summary>    
    public class LazyResourceTest : TestBase, IDisposable
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
        ///   Tests rate and upkeep being zero
        /// </summary>
        [Fact]
        public void TestZero()
        {
            var resource = new LazyValue(0, begin, 0, 0);
            SystemClock.SetClock(begin.AddHours(12));
            Assert.Equal(resource.Value, 0);
        }

        /// <summary>
        ///   Tests having positive rate but no upkeep
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
        ///   Tests having positive upkeep but no rate
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
        ///   Tests having rate higher than upkeep
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
        ///   Tests having upkeep higher than rate
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
        ///   Test equal upkeep and rate
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
        ///   Tests changing the rate
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
        ///   Tests changing the upkeep
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
        ///   Tests having positive rate but no upkeep and secs per unit modified
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
        ///   Tests having rate higher than upkeep and secs per unit
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
    }
}