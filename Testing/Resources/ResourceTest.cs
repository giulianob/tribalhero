#region

using Game.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Testing.Resources
{
    /// <summary>
    ///   Summary description for LazyResourceTest
    /// </summary>
    [TestClass]
    public class ResourceTest : TestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        /// <summary>
        ///   Tests main resource constructor
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);

            var resource2 = new Resource {Crop = 1, Gold = 2, Iron = 3, Wood = 4, Labor = 5};

            Assert.IsTrue(resource1.CompareTo(resource2) == 0);
        }

        /// <summary>
        ///   Tests resources are comparing properly
        /// </summary>
        [TestMethod]
        public void TestEqual()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            var resource2 = new Resource(1, 2, 3, 4, 5);

            Assert.IsTrue(resource1.CompareTo(resource2) == 0);
        }

        /// <summary>
        ///   Tests resource copy constructor
        /// </summary>
        [TestMethod]
        public void TestCopyConstructor()
        {
            var source = new Resource(1, 2, 3, 4, 5);
            var target = new Resource(source);

            Assert.IsTrue(source.CompareTo(target) == 0);
        }

        [TestMethod]
        public void TestMaxAffordableCrop()
        {
            var resource1 = new Resource(3, 0, 0, 0, 0);
            Assert.IsTrue(resource1.MaxAffordable(new Resource(3, 0, 0, 0, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableGold()
        {
            var resource1 = new Resource(0, 3, 0, 0, 0);
            Assert.IsTrue(resource1.MaxAffordable(new Resource(0, 3, 0, 0, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableIron()
        {
            var resource1 = new Resource(0, 0, 3, 0, 0);
            Assert.IsTrue(resource1.MaxAffordable(new Resource(0, 0, 3, 0, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableWood()
        {
            var resource1 = new Resource(0, 0, 0, 3, 0);
            Assert.IsTrue(resource1.MaxAffordable(new Resource(0, 0, 0, 3, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableLabor()
        {
            var resource1 = new Resource(0, 0, 0, 0, 3);
            Assert.IsTrue(resource1.MaxAffordable(new Resource(0, 0, 0, 0, 3)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableMultiple()
        {
            var resource1 = new Resource(2, 4, 6, 8, 10);
            Assert.IsTrue(resource1.MaxAffordable(new Resource(1, 2, 3, 4, 5)) == 2);
        }

        [TestMethod]
        public void TestMaxAffordableMax()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Assert.IsTrue(resource1.MaxAffordable(new Resource(0, 0, 0, 0, 0)) == int.MaxValue);
        }

        [TestMethod]
        public void TestSubtractExact()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.Subtract(new Resource(1, 2, 3, 4, 5), out actual);

            Assert.IsTrue(resource1.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
            Assert.IsTrue(actual.CompareTo(new Resource(1, 2, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestSubtractOverflow()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.Subtract(new Resource(10, 10, 10, 10, 10), out actual);

            Assert.IsTrue(resource1.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
            Assert.IsTrue(actual.CompareTo(new Resource(1, 2, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestSubtractLess()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.Subtract(new Resource(0, 1, 2, 3, 4), out actual);

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 1, 1, 1, 1)) == 0);
            Assert.IsTrue(actual.CompareTo(new Resource(0, 1, 2, 3, 4)) == 0);
        }

        [TestMethod]
        public void TestAddCrop()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(10, 0, 0, 0, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(11, 2, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddGold()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 10, 0, 0, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 12, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddIron()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 0, 10, 0, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 2, 13, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddWood()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 0, 0, 10, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 2, 3, 14, 5)) == 0);
        }

        [TestMethod]
        public void TestAddLabor()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 0, 0, 0, 10));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 2, 3, 4, 15)) == 0);
        }

        [TestMethod]
        public void TestAddWithCapAndCapIsHigher()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource returning;
            Resource actual;
            resource1.Add(new Resource(1, 2, 3, 4, 5), new Resource(10), out actual, out returning);

            Assert.IsTrue(resource1.CompareTo(new Resource(2, 4, 6, 8, 10)) == 0);
            Assert.IsTrue(returning.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
            Assert.IsTrue(actual.CompareTo(new Resource(1, 2, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddWithCapAndCapIsLowerThanBase()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource returning;
            Resource actual;
            resource1.Add(new Resource(1, 2, 3, 4, 5), new Resource(5), out actual, out returning);

            Assert.IsTrue(resource1.CompareTo(new Resource(2, 4, 5, 5, 5)) == 0);
            Assert.IsTrue(returning.CompareTo(new Resource(0, 0, 1, 3, 5)) == 0);
            Assert.IsTrue(actual.CompareTo(new Resource(1, 2, 2, 1, 0)) == 0);
        }

        [TestMethod]
        public void TestAddWithCapAndInitialIsHigher()
        {
            var resource1 = new Resource(0, 20, 30, 40, 50);
            Resource returning;
            Resource actual;
            resource1.Add(new Resource(1, 20, 30, 40, 50), new Resource(5), out actual, out returning);

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 5, 5, 5, 5)) == 0);
            Assert.IsTrue(returning.CompareTo(new Resource(0, 35, 55, 75, 95)) == 0);
            Assert.IsTrue(actual.CompareTo(new Resource(1, 0, 0, 0, 0)) == 0);
        }
    }
}