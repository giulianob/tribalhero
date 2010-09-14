using System;
using Game.Data;
using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Testing.Channel {
    /// <summary>
    /// Summary description for ChannelTest
    /// </summary>
    [TestClass]
    public class CityTest {

        [TestInitialize]
        public void TestInitialize() {
        }
        
        [TestCleanup]
        public void TestCleanup() { }        

        [TestMethod]
        public void TestValidName() {
            Assert.IsTrue(City.IsNameValid("Ab 1c d1 2"));
        }

        [TestMethod]
        public void TestLowerCase() {
            Assert.IsTrue(City.IsNameValid("ab 1c d1 2"));
        }

        [TestMethod]
        public void TestStartWithNumber() {
            Assert.IsFalse(City.IsNameValid("1Hello World"));
        }

        [TestMethod]
        public void TestTooShort() {
            Assert.IsFalse(City.IsNameValid("a"));
        }

        [TestMethod]
        public void TestJustShortEnough() {
            Assert.IsTrue(City.IsNameValid("xxx"));
        }

        [TestMethod]
        public void TestTooLong() {
            Assert.IsFalse(City.IsNameValid("xxxxxxxxxxxxxxxxx"));
        }

        [TestMethod]
        public void TestJustLongEnough() {
            Assert.IsTrue(City.IsNameValid("xxxxxxxxxxxxxxxx"));
        }

        [TestMethod]
        public void TestNewLine() {
            Assert.IsFalse(City.IsNameValid("xx\nxxx"));
        }
    }
}
