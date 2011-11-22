#region

using FluentAssertions;
using Game.Data;
using Xunit;

#endregion

namespace Testing.MapMath
{
    /// <summary>
    ///   Summary description for ChannelTest
    /// </summary>    
    public class RadiusTest : TestBase
    {
        [Fact]
        public void TestSelf()
        {
            SimpleGameObject.RadiusDistance(13, 13, 13, 13).Should().Be(0);
            SimpleGameObject.RadiusDistance(13, 14, 13, 14).Should().Be(0);
            SimpleGameObject.RadiusDistance(14, 13, 14, 13).Should().Be(0);
            SimpleGameObject.RadiusDistance(14, 14, 14, 14).Should().Be(0);
        }

        [Fact]
        public void TestEvenyEveny1()
        {
            //Even y Even y1
            SimpleGameObject.RadiusDistance(13, 14, 12, 14).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 14, 14, 14).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 14, 13, 12).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 14, 13, 16).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 14, 14, 16).Should().Be(3);
            SimpleGameObject.RadiusDistance(13, 14, 13, 18).Should().Be(5);
            SimpleGameObject.RadiusDistance(13, 14, 16, 14).Should().Be(8);
        }

        [Fact]
        public void TestOddyOddy1()
        {
            //Odd y Odd y1
            SimpleGameObject.RadiusDistance(13, 15, 12, 15).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 15, 14, 15).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 15, 13, 13).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 15, 13, 17).Should().Be(2);
            SimpleGameObject.RadiusDistance(13, 15, 14, 17).Should().Be(3);
            SimpleGameObject.RadiusDistance(13, 15, 13, 19).Should().Be(5);
            SimpleGameObject.RadiusDistance(13, 15, 16, 15).Should().Be(8);
        }

        [Fact]
        public void TestEvenyOddy1()
        {
            //Even y Odd y1
            SimpleGameObject.RadiusDistance(13, 14, 12, 13).Should().Be(1);
            SimpleGameObject.RadiusDistance(13, 14, 13, 13).Should().Be(1);
            SimpleGameObject.RadiusDistance(13, 14, 12, 15).Should().Be(1);
            SimpleGameObject.RadiusDistance(13, 14, 13, 15).Should().Be(1);

            SimpleGameObject.RadiusDistance(13, 14, 13, 17).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 14, 13, 19).Should().Be(7);
            SimpleGameObject.RadiusDistance(13, 14, 14, 15).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 14, 15, 15).Should().Be(7);

            SimpleGameObject.RadiusDistance(13, 14, 14, 13).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 14, 15, 13).Should().Be(7);
            SimpleGameObject.RadiusDistance(13, 14, 11, 17).Should().Be(5);
            SimpleGameObject.RadiusDistance(13, 14, 13, 11).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 14, 13, 9).Should().Be(7);

            SimpleGameObject.RadiusDistance(13, 14, 11, 13).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 14, 10, 13).Should().Be(7);
            SimpleGameObject.RadiusDistance(13, 14, 12, 11).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 14, 12, 9).Should().Be(7);

            SimpleGameObject.RadiusDistance(13, 14, 12, 17).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 14, 11, 13).Should().Be(4);
        }

        [Fact]
        public void TestTileRadius()
        {
            /***********************************************************
                         13,12  |  14,12 
          11,13     12,13  |  13,13  |  14,13  |  15,13
               12,14  |  13,14  |  14,14  |  15,14  | 16,14
          11,15  |  12,15  |  (13,15)  |  14,15
               12,16  |  13,16  |  14,16
                    12,17  |  13,17  | 14,17
                         13,18     14,18
             *********************************************************/

            SimpleGameObject.RadiusDistance(13, 15, 13, 14).Should().Be(1);
            SimpleGameObject.RadiusDistance(13, 15, 14, 14).Should().Be(1);
            SimpleGameObject.RadiusDistance(13, 15, 14, 16).Should().Be(1);
            SimpleGameObject.RadiusDistance(13, 15, 13, 16).Should().Be(1);

            SimpleGameObject.RadiusDistance(13, 15, 13, 12).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 15, 12, 14).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 15, 14, 12).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 15, 15, 14).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 15, 12, 16).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 15, 13, 18).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 15, 14, 18).Should().Be(4);
            SimpleGameObject.RadiusDistance(13, 15, 15, 16).Should().Be(4);

            SimpleGameObject.RadiusDistance(13, 15, 16, 14).Should().Be(7);
            SimpleGameObject.RadiusDistance(13, 15, 14, 20).Should().Be(7);
            SimpleGameObject.RadiusDistance(13, 15, 13, 20).Should().Be(7);
            SimpleGameObject.RadiusDistance(13, 15, 11, 14).Should().Be(7);
        }
    }
}