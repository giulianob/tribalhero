#region

using Common.Testing;
using FluentAssertions;
using Game.Map;
using Xunit;
using Xunit.Extensions;

#endregion

namespace Testing.MapMathTests
{
    public class RadiusTest
    {
        private readonly TileLocator cacheForTestIsOverlappingWithCache;

        public RadiusTest()
        {
            cacheForTestIsOverlappingWithCache = new TileLocator();
        }

        [Fact]
        public void TestSelf()
        {
            new TileLocator().RadiusDistance(13, 13, 13, 13).Should().Be(0);
            new TileLocator().RadiusDistance(13, 14, 13, 14).Should().Be(0);
            new TileLocator().RadiusDistance(14, 13, 14, 13).Should().Be(0);
            new TileLocator().RadiusDistance(14, 14, 14, 14).Should().Be(0);
        }

        [Fact]
        public void TestEvenyEveny1()
        {
            //Even y Even y1
            new TileLocator().RadiusDistance(13, 14, 12, 14).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 14, 14).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 13, 12).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 13, 16).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 14, 16).Should().Be(3);
            new TileLocator().RadiusDistance(13, 14, 13, 18).Should().Be(5);
            new TileLocator().RadiusDistance(13, 14, 16, 14).Should().Be(8);
        }

        [Fact]
        public void TestOddyOddy1()
        {
            //Odd y Odd y1
            new TileLocator().RadiusDistance(13, 15, 12, 15).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 14, 15).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 13, 13).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 13, 17).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 14, 17).Should().Be(3);
            new TileLocator().RadiusDistance(13, 15, 13, 19).Should().Be(5);
            new TileLocator().RadiusDistance(13, 15, 16, 15).Should().Be(8);
        }

        [Fact]
        public void TestEvenyOddy1()
        {
            //Even y Odd y1
            new TileLocator().RadiusDistance(13, 14, 12, 13).Should().Be(1);
            new TileLocator().RadiusDistance(13, 14, 13, 13).Should().Be(1);
            new TileLocator().RadiusDistance(13, 14, 12, 15).Should().Be(1);
            new TileLocator().RadiusDistance(13, 14, 13, 15).Should().Be(1);

            new TileLocator().RadiusDistance(13, 14, 13, 17).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 13, 19).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 14, 15).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 15, 15).Should().Be(7);

            new TileLocator().RadiusDistance(13, 14, 14, 13).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 15, 13).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 11, 17).Should().Be(5);
            new TileLocator().RadiusDistance(13, 14, 13, 11).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 13, 9).Should().Be(7);

            new TileLocator().RadiusDistance(13, 14, 11, 13).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 10, 13).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 12, 11).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 12, 9).Should().Be(7);

            new TileLocator().RadiusDistance(13, 14, 12, 17).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 11, 13).Should().Be(4);
        }

        [Fact]
        public void TestTileRadius()
        {
            new TileLocator().RadiusDistance(12, 16, 11, 13).Should().Be(4);
            new TileLocator().RadiusDistance(12, 16, 12, 13).Should().Be(4);

            new TileLocator().RadiusDistance(13, 15, 13, 14).Should().Be(1);
            new TileLocator().RadiusDistance(13, 15, 14, 14).Should().Be(1);
            new TileLocator().RadiusDistance(13, 15, 14, 16).Should().Be(1);
            new TileLocator().RadiusDistance(13, 15, 13, 16).Should().Be(1);

            new TileLocator().RadiusDistance(13, 15, 13, 12).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 12, 14).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 14, 12).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 15, 14).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 12, 16).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 13, 18).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 14, 18).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 15, 16).Should().Be(4);

            new TileLocator().RadiusDistance(13, 15, 16, 14).Should().Be(7);
            new TileLocator().RadiusDistance(13, 15, 14, 20).Should().Be(7);
            new TileLocator().RadiusDistance(13, 15, 13, 20).Should().Be(7);
            new TileLocator().RadiusDistance(13, 15, 11, 14).Should().Be(7);
        }

        [Theory(Skip = "Intensive test.. only run if you need to"), CsvData("MapMath/overlapping_test_data.csv")]
        
        public void TestGeneratedValues(uint x1,
                                        uint y1,
                                        byte r1,
                                        uint x2,
                                        uint y2,
                                        byte r2,
                                        bool overlapping,
                                        byte distance)
        {
            new TileLocator().RadiusDistance(x1, y1, x2, y2).Should().Be(distance);
        }

        [Theory(Skip = "Intensive test.. only run if you need to"), CsvData("MapMath/overlapping_test_data.csv")]
        
        public void TestIsOverlapping(uint x1,
                                      uint y1,
                                      byte r1,
                                      uint x2,
                                      uint y2,
                                      byte r2,
                                      bool overlapping,
                                      byte distance)
        {
            new TileLocator().IsOverlapping(new Position(x1, y1), r1, new Position(x2, y2), r2)
                               .Should()
                               .Be(overlapping);
        }

        [Theory(Skip = "Intensive test.. only run if you need to"), CsvData("MapMath/overlapping_test_data.csv")]
        public void TestIsOverlappingAllPointsWithCache(uint x1,
                                                        uint y1,
                                                        byte r1,
                                                        uint x2,
                                                        uint y2,
                                                        byte r2,
                                                        bool overlapping,
                                                        byte distance)
        {
            cacheForTestIsOverlappingWithCache.IsOverlapping(new Position(x1, y1), r1, new Position(x2, y2), r2)
                                              .Should()
                                              .Be(overlapping);
        }

        [Fact]
        public void TestIsOverlappingWithCache_WhenNotClose_ShouldNotHitCache()
        {
            var tileLocator = new TileLocator();

            tileLocator.IsOverlapping(new Position(50, 50), 1, new Position(50, 47), 0);

            tileLocator.OverlappingCache.Should().HaveCount(0);
        }
    }
}