using System;
using Game.Data;
using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Resources {
    /// <summary>
    /// Summary description for LazyResourceTest
    /// </summary>
    [TestClass]
    public class ResourceTest {

        [TestInitialize]
        public void TestInitialize() {
            
        }

        [TestCleanup]
        public void TestCleanup() {
            
        }

        /// <summary>
        /// Tests main resource constructor
        /// </summary>
        [TestMethod]
        public void TestConstructor() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);

            Resource resource2 = new Resource {
                                                  Crop = 1,
                                                  Gold = 2,
                                                  Iron = 3,
                                                  Wood = 4,
                                                  Labor = 5
                                              };            

            Assert.IsTrue(resource1.CompareTo(resource2) == 0);
        }        

        /// <summary>
        /// Tests resources are comparing properly
        /// </summary>
        [TestMethod]
        public void TestEqual() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            Resource resource2 = new Resource(1, 2, 3, 4, 5);
            
            Assert.IsTrue(resource1.CompareTo(resource2) == 0);            
        }

        /// <summary>
        /// Tests resource copy constructor
        /// </summary>
        [TestMethod]
        public void TestCopyConstructor() {          
            Resource source = new Resource(1, 2, 3, 4, 5);
            Resource target = new Resource(source);

            Assert.IsTrue(source.CompareTo(target) == 0);
        }

        [TestMethod]
        public void TestMaxAffordableCrop() {
            Resource resource1 = new Resource(3, 0, 0, 0, 0);
            Assert.IsTrue(resource1.maxAffordable(new Resource(3, 0, 0, 0, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableGold() {
            Resource resource1 = new Resource(0, 3, 0, 0, 0);
            Assert.IsTrue(resource1.maxAffordable(new Resource(0, 3, 0, 0, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableIron() {
            Resource resource1 = new Resource(0, 0, 3, 0, 0);
            Assert.IsTrue(resource1.maxAffordable(new Resource(0, 0, 3, 0, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableWood() {
            Resource resource1 = new Resource(0, 0, 0, 3, 0);
            Assert.IsTrue(resource1.maxAffordable(new Resource(0, 0, 0, 3, 0)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableLabor() {
            Resource resource1 = new Resource(0, 0, 0, 0, 3);
            Assert.IsTrue(resource1.maxAffordable(new Resource(0, 0, 0, 0, 3)) == 1);
        }

        [TestMethod]
        public void TestMaxAffordableMultiple() {
            Resource resource1 = new Resource(2, 4, 6, 8, 10);
            Assert.IsTrue(resource1.maxAffordable(new Resource(1, 2, 3, 4, 5)) == 2);
        }

        [TestMethod]
        public void TestMaxAffordableMax() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            Assert.IsTrue(resource1.maxAffordable(new Resource(0, 0, 0, 0, 0)) == int.MaxValue);
        }

        [TestMethod]
        public void TestSubtractSimple() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.subtract(new Resource(1, 2, 3, 4, 5), out actual);

            Assert.IsTrue(resource1.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
        }

        [TestMethod]
        public void TestSubtractOverflow() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.subtract(new Resource(10, 10, 10, 10, 10), out actual);

            Assert.IsTrue(resource1.CompareTo(new Resource(0, 0, 0, 0, 0)) == 0);
        }

        [TestMethod]
        public void TestSubtractActual() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            Resource actual;
            resource1.subtract(new Resource(10, 10, 10, 10, 10), out actual);

            Assert.IsTrue(actual.CompareTo(new Resource(1, 2, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddCrop() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);            
            resource1.add(new Resource(10, 0, 0, 0, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(11, 2, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddGold() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.add(new Resource(0, 10, 0, 0, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 12, 3, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddIron() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.add(new Resource(0, 0, 10, 0, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 2, 13, 4, 5)) == 0);
        }

        [TestMethod]
        public void TestAddWood() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.add(new Resource(0, 0, 0, 10, 0));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 2, 3, 14, 5)) == 0);
        }

        [TestMethod]
        public void TestAddLabor() {
            Resource resource1 = new Resource(1, 2, 3, 4, 5);
            resource1.add(new Resource(0, 0, 0, 0, 10));

            Assert.IsTrue(resource1.CompareTo(new Resource(1, 2, 3, 4, 15)) == 0);
        }
    }
}