#region

using System;
using Game.Data;
using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Testing.Resources
{
    /// <summary>
    ///   Summary description for LazyResourceTest
    /// </summary>
    [TestClass]
    public class AggressiveLazyResourceTest
    {
        private DateTime begin = DateTime.UtcNow;

        [TestInitialize]
        public void TestInitialize()
        {
            SystemClock.SetClock(begin);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            SystemClock.ResyncClock();
        }

        /// <summary>
        ///   Tests rate and upkeep being zero
        /// </summary>
        [TestMethod]
        public void TestZero()
        {
            var resource = new AggressiveLazyValue(0, begin, 0, 0);
            SystemClock.SetClock(begin.AddHours(12));
            Assert.AreEqual(resource.Value, 0);
        }

        /// <summary>
        ///   Tests having positive rate but no upkeep
        /// </summary>
        [TestMethod]
        public void TestPositiveRate()
        {
            var resource = new AggressiveLazyValue(0, begin, 100, 0);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.AreEqual(resource.Value, 50);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.AreEqual(resource.Value, 100);
        }

        /// <summary>
        ///   Tests having positive upkeep but no rate
        /// </summary>
        [TestMethod]
        public void TestPositiveUpkeep()
        {
            var resource = new AggressiveLazyValue(100, begin, 0, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.AreEqual(50, resource.Value);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.AreEqual(0, resource.Value);
        }

        /// <summary>
        ///   Tests having rate higher than upkeep
        /// </summary>
        [TestMethod]
        public void TestPositiveRateGreaterThanUpkeep()
        {
            var resource = new AggressiveLazyValue(0, begin, 100, 50);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.AreEqual(resource.Value, 25);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.AreEqual(resource.Value, 50);
        }

        /// <summary>
        ///   Tests having upkeep higher than rate
        /// </summary>
        [TestMethod]
        public void TestPositiveUpkeepGreaterThanRate()
        {
            var resource = new AggressiveLazyValue(100, begin, 50, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.AreEqual(resource.Value, 75);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.AreEqual(resource.Value, 50);
        }

        /// <summary>
        ///   Test equal upkeep and rate
        /// </summary>
        [TestMethod]
        public void TestEqualUpkeepAndRate()
        {
            var resource = new AggressiveLazyValue(100, begin, 100, 100);
            SystemClock.SetClock(begin.AddMinutes(30));
            Assert.AreEqual(resource.Value, 100);

            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.AreEqual(resource.Value, 100);
        }

        /// <summary>
        ///   Tests changing the rate
        /// </summary>
        [TestMethod]
        public void TestChangeRate()
        {
            var resource = new AggressiveLazyValue(0, begin, 0, 0);

            // Set the rate higher            
            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Rate = 100;
            Assert.AreEqual(resource.Value, 0);

            // Probe later
            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.AreEqual(resource.Value, 50);
        }

        /// <summary>
        ///   Tests changing the upkeep
        /// </summary>
        [TestMethod]
        public void TestChangeUpkeep()
        {
            var resource = new AggressiveLazyValue(100, begin, 100, 0);

            // Set the upkeep higher
            SystemClock.SetClock(begin.AddMinutes(30));
            resource.Upkeep = 50;
            resource.Rate = 0;
            Assert.AreEqual(resource.Value, 150);

            // Probe later
            SystemClock.SetClock(begin.AddMinutes(60));
            Assert.AreEqual(resource.Value, 125);
        }
    }
}