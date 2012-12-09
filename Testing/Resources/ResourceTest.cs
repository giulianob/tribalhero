#region

using Game.Data;
using Xunit;

#endregion

namespace Testing.Resources
{
    /// <summary>
    ///     Summary description for LazyResourceTest
    /// </summary>
    public class ResourceTest
    {
        /// <summary>
        ///     Tests main resource constructor
        /// </summary>
        [Fact]
        public void TestConstructor()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);

            var resource2 = new Resource {Crop = 1, Gold = 2, Iron = 3, Wood = 4, Labor = 5};

            Assert.True(resource1.CompareTo(resource2) == 0);
        }

        /// <summary>
        ///     Tests resources are comparing properly
        /// </summary>
        [Fact]
        public void TestEqual()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            var resource2 = new Resource(1, 2, 3, 4, 5);

            Assert.True(resource1.CompareTo(resource2) == 0);
        }

        /// <summary>
        ///     Tests resource copy constructor
        /// </summary>
        [Fact]
        public void TestCopyConstructor()
        {
            var source = new Resource(1, 2, 3, 4, 5);
            var target = new Resource(source);

            Assert.True(source.CompareTo(target) == 0);
        }

        [Fact]
        public void TestMaxAffordableCrop()
        {
            var resource1 = new Resource(3, 0, 0, 0, 0);
            Assert.True(resource1.MaxAffordable(new Resource(3, 0, 0, 0, 0)) == 1);
        }

        [Fact]
        public void TestMaxAffordableGold()
        {
            var resource1 = new Resource(0, 3, 0, 0, 0);
            Assert.True(resource1.MaxAffordable(new Resource(0, 3, 0, 0, 0)) == 1);
        }

        [Fact]
        public void TestMaxAffordableIron()
        {
            var resource1 = new Resource(0, 0, 3, 0, 0);
            Assert.True(resource1.MaxAffordable(new Resource(0, 0, 3, 0, 0)) == 1);
        }

        [Fact]
        public void TestMaxAffordableWood()
        {
            var resource1 = new Resource(0, 0, 0, 3, 0);
            Assert.True(resource1.MaxAffordable(new Resource(0, 0, 0, 3, 0)) == 1);
        }

        [Fact]
        public void TestMaxAffordableLabor()
        {
            var resource1 = new Resource(0, 0, 0, 0, 3);
            Assert.True(resource1.MaxAffordable(new Resource(0, 0, 0, 0, 3)) == 1);
        }

        [Fact]
        public void TestMaxAffordableMultiple()
        {
            var resource1 = new Resource(2, 4, 6, 8, 10);
            Assert.True(resource1.MaxAffordable(new Resource(1, 2, 3, 4, 5)) == 2);
        }

        [Fact]
        public void TestMaxAffordableMax()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Assert.True(resource1.MaxAffordable(new Resource(0, 0, 0, 0, 0)) == int.MaxValue);
        }

        [Fact]
        public void TestSubtractExact()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.Subtract(new Resource(1, 2, 3, 4, 5), out actual);

            Assert.True(resource1.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
            Assert.True(actual.CompareTo(new Resource(1, 2, 3, 4, 5)) == 0);
        }

        [Fact]
        public void TestSubtractOverflow()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.Subtract(new Resource(10, 10, 10, 10, 10), out actual);

            Assert.True(resource1.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
            Assert.True(actual.CompareTo(new Resource(1, 2, 3, 4, 5)) == 0);
        }

        [Fact]
        public void TestSubtractLess()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.Subtract(new Resource(0, 1, 2, 3, 4), out actual);

            Assert.True(resource1.CompareTo(new Resource(1, 1, 1, 1, 1)) == 0);
            Assert.True(actual.CompareTo(new Resource(0, 1, 2, 3, 4)) == 0);
        }

        [Fact]
        public void TestAddCrop()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(10, 0, 0, 0, 0));

            Assert.True(resource1.CompareTo(new Resource(11, 2, 3, 4, 5)) == 0);
        }

        [Fact]
        public void TestAddGold()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 10, 0, 0, 0));

            Assert.True(resource1.CompareTo(new Resource(1, 12, 3, 4, 5)) == 0);
        }

        [Fact]
        public void TestAddIron()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 0, 10, 0, 0));

            Assert.True(resource1.CompareTo(new Resource(1, 2, 13, 4, 5)) == 0);
        }

        [Fact]
        public void TestAddWood()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 0, 0, 10, 0));

            Assert.True(resource1.CompareTo(new Resource(1, 2, 3, 14, 5)) == 0);
        }

        [Fact]
        public void TestAddLabor()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.Add(new Resource(0, 0, 0, 0, 10));

            Assert.True(resource1.CompareTo(new Resource(1, 2, 3, 4, 15)) == 0);
        }

        [Fact]
        public void TestAddWithCapAndCapIsHigher()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource returning;
            Resource actual;
            resource1.Add(new Resource(1, 2, 3, 4, 5), new Resource(10), out actual, out returning);

            Assert.True(resource1.CompareTo(new Resource(2, 4, 6, 8, 10)) == 0);
            Assert.True(returning.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
            Assert.True(actual.CompareTo(new Resource(1, 2, 3, 4, 5)) == 0);
        }

        [Fact]
        public void TestAddWithCapAndCapIsLowerThanBase()
        {
            var resource1 = new Resource(1, 2, 3, 4, 5);
            Resource returning;
            Resource actual;
            resource1.Add(new Resource(1, 2, 3, 4, 5), new Resource(5), out actual, out returning);

            Assert.True(resource1.CompareTo(new Resource(2, 4, 5, 5, 5)) == 0);
            Assert.True(returning.CompareTo(new Resource(0, 0, 1, 3, 5)) == 0);
            Assert.True(actual.CompareTo(new Resource(1, 2, 2, 1, 0)) == 0);
        }

        [Fact]
        public void TestAddWithCapAndInitialIsHigher()
        {
            var resource1 = new Resource(0, 20, 30, 40, 50);
            Resource returning;
            Resource actual;
            resource1.Add(new Resource(1, 20, 30, 40, 50), new Resource(5), out actual, out returning);

            Assert.True(resource1.CompareTo(new Resource(1, 5, 5, 5, 5)) == 0);
            Assert.True(returning.CompareTo(new Resource(0, 35, 55, 75, 95)) == 0);
            Assert.True(actual.CompareTo(new Resource(1, 0, 0, 0, 0)) == 0);
        }
    }
}