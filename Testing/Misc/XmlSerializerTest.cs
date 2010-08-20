using System.Collections.Generic;
using Game.Data;
using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.MapMath {
    /// <summary>
    /// Summary description for ChannelTest
    /// </summary>
    [TestClass]
    public class XmlSerializerTest {

        [TestInitialize]
        public void TestInitialize() {
        }

        [TestCleanup]
        public void TestCleanup() { }

        [TestMethod]
        public void TestDeserializeComplexEmpty() {
            List<object[]> list = XMLSerializer.DeserializeComplexList("<List />");

            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void TestDeserializeComplexEmptyProperty() {
            List<object[]> list = XMLSerializer.DeserializeComplexList("<List><Properties /></List>");

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0, list[0].Length);
        }

        [TestMethod]
        public void TestDeserializeComplexEmptyPropertyMultiple() {
            List<object[]> list = XMLSerializer.DeserializeComplexList("<List><Properties /><Properties /></List>");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(0, list[0].Length);
            Assert.AreEqual(0, list[1].Length);
        }
    }
}