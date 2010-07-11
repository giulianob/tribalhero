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
        public void TestSelf() {
            Assert.AreEqual(0, GameObject.RadiusDistance(13, 13, 13, 13));
            Assert.AreEqual(0, GameObject.RadiusDistance(13, 14, 13, 14));
            Assert.AreEqual(0, GameObject.RadiusDistance(14, 13, 14, 13));
            Assert.AreEqual(0, GameObject.RadiusDistance(14, 14, 14, 14));
        }

        [TestMethod]
        public void TestEvenyEveny1() {
            //Even y Even y1
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 14, 12, 14));
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 14, 14, 14));
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 14, 13, 12));
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 14, 13, 16));
            Assert.AreEqual(3, GameObject.RadiusDistance(13, 14, 14, 16));
            Assert.AreEqual(5, GameObject.RadiusDistance(13, 14, 13, 18));
            Assert.AreEqual(8, GameObject.RadiusDistance(13, 14, 16, 14));
        }
        [TestMethod]
        public void TesOddyOddy1() {
            //Odd y Odd y1
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 15, 12, 15));
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 15, 14, 15));
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 15, 13, 13));
            Assert.AreEqual(2, GameObject.RadiusDistance(13, 15, 13, 17));
            Assert.AreEqual(3, GameObject.RadiusDistance(13, 15, 14, 17));
            Assert.AreEqual(5, GameObject.RadiusDistance(13, 15, 13, 19));
            Assert.AreEqual(8, GameObject.RadiusDistance(13, 15, 16, 15));
        }

        [TestMethod]
        public void TestEvenyOddy1() {

            //Even y Odd y1
            Assert.AreEqual(1, GameObject.RadiusDistance(13, 14, 12, 13));
            Assert.AreEqual(1, GameObject.RadiusDistance(13, 14, 13, 13));
            Assert.AreEqual(1, GameObject.RadiusDistance(13, 14, 12, 15));
            Assert.AreEqual(1, GameObject.RadiusDistance(13, 14, 13, 15));

            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 13, 17));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 14, 13, 19));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 14, 15));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 14, 15, 15));

            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 14, 13));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 14, 15, 13));
            Assert.AreEqual(5, GameObject.RadiusDistance(13, 14, 11, 17));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 13, 11));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 14, 13, 9));

            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 11, 13));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 14, 10, 13));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 12, 11));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 14, 12, 9));

            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 12, 17));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 14, 11, 13));

        }

        [TestMethod]
        public void TestTileRadius() {

            /***********************************************************
                         13,12  |  14,12 
          11,13     12,13  |  13,13  |  14,13  |  15,13
               12,14  |  13,14  |  14,14  |  15,14  | 16,14
          11,15  |  12,15  |  (13,15)  |  14,15
               12,16  |  13,16  |  14,16
                    12,17  |  13,17  | 14,17
                         13,18     14,18
             *********************************************************/

            Assert.AreEqual(1, GameObject.RadiusDistance(13, 15, 13, 14));
            Assert.AreEqual(1, GameObject.RadiusDistance(13, 15, 14, 14));
            Assert.AreEqual(1, GameObject.RadiusDistance(13, 15, 14, 16));
            Assert.AreEqual(1, GameObject.RadiusDistance(13, 15, 13, 16));

            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 13, 12));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 12, 14));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 14, 12));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 15, 14));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 12, 16));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 13, 18));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 14, 18));
            Assert.AreEqual(4, GameObject.RadiusDistance(13, 15, 15, 16));

            Assert.AreEqual(7, GameObject.RadiusDistance(13, 15, 16, 14));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 15, 14, 20));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 15, 13, 20));
            Assert.AreEqual(7, GameObject.RadiusDistance(13, 15, 11, 14));
        }
    }
}