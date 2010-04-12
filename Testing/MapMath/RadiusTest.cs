using Game.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.MapMath {
    /// <summary>
    /// Summary description for ChannelTest
    /// </summary>
    [TestClass]
    public class RadiusTest {

        [TestInitialize]
        public void TestInitialize() {
        }

        [TestCleanup]
        public void TestCleanup() { }

        [TestMethod]
        public void TestTileRadius() {            
            Assert.AreEqual(1f, GameObject.RadiusDistance(13, 14, 12, 13));
            Assert.AreEqual(1f, GameObject.RadiusDistance(13, 14, 13, 13));
            Assert.AreEqual(1f, GameObject.RadiusDistance(13, 14, 12, 15));
            Assert.AreEqual(1f, GameObject.RadiusDistance(13, 14, 13, 15));

            Assert.AreEqual(1.5f, GameObject.RadiusDistance(13, 14, 12, 14));
            Assert.AreEqual(1.5f, GameObject.RadiusDistance(13, 14, 14, 14));
            Assert.AreEqual(1.5f, GameObject.RadiusDistance(13, 14, 13, 12));
            Assert.AreEqual(1.5f, GameObject.RadiusDistance(13, 14, 13, 16));

            Assert.AreEqual(2.0f, GameObject.RadiusDistance(13, 14, 14, 16));

            Assert.AreEqual(2.5f, GameObject.RadiusDistance(13, 14, 13, 17));

            Assert.AreEqual(3.0f, GameObject.RadiusDistance(13, 14, 13, 18));

            Assert.AreEqual(3.5f, GameObject.RadiusDistance(13, 14, 13, 18));

            Assert.AreEqual(4.5f, GameObject.RadiusDistance(13, 14, 16, 14));

        }
    }
}