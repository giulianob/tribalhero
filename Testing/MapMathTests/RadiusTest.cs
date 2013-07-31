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
        private readonly ITileLocator cacheForTestIsOverlappingWithCache;

        public RadiusTest()
        {
            cacheForTestIsOverlappingWithCache = new TileLocator();
        }

        [Fact]
        public void TestSelf()
        {
            new TileLocator().RadiusDistance(13, 13, 1, 13, 13, 1).Should().Be(0);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 14, 1).Should().Be(0);
            new TileLocator().RadiusDistance(14, 13, 1, 14, 13, 1).Should().Be(0);
            new TileLocator().RadiusDistance(14, 14, 1, 14, 14, 1).Should().Be(0);
        }

        [Fact]
        public void TestEvenyEveny1()
        {
            //Even y Even y1
            new TileLocator().RadiusDistance(13, 14, 1, 12, 14, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 1, 14, 14, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 12, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 16, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 14, 1, 14, 16, 1).Should().Be(3);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 18, 1).Should().Be(5);
            new TileLocator().RadiusDistance(13, 14, 1, 16, 14, 1).Should().Be(8);
        }

        [Fact]
        public void TestOddyOddy1()
        {
            //Odd y Odd y1
            new TileLocator().RadiusDistance(13, 15, 1, 12, 15, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 1, 14, 15, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 1, 13, 13, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 1, 13, 17, 1).Should().Be(2);
            new TileLocator().RadiusDistance(13, 15, 1, 14, 17, 1).Should().Be(3);
            new TileLocator().RadiusDistance(13, 15, 1, 13, 19, 1).Should().Be(5);
            new TileLocator().RadiusDistance(13, 15, 1, 16, 15, 1).Should().Be(8);
        }

        [Fact]
        public void TestEvenyOddy1()
        {
            //Even y Odd y1
            new TileLocator().RadiusDistance(13, 14, 1, 12, 13, 1).Should().Be(1);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 13, 1).Should().Be(1);
            new TileLocator().RadiusDistance(13, 14, 1, 12, 15, 1).Should().Be(1);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 15, 1).Should().Be(1);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 17, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 19, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 1, 14, 15, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 1, 15, 15, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 1, 14, 13, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 1, 15, 13, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 1, 11, 17, 1).Should().Be(5);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 11, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 1, 13, 9, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 1, 11, 13, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 1, 10, 13, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 1, 12, 11, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 1, 12, 9, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 14, 1, 12, 17, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 14, 1, 11, 13, 1).Should().Be(4);
        }

        [Fact]
        public void TestTileRadius()
        {
            new TileLocator().RadiusDistance(12, 16, 1, 11, 13, 1).Should().Be(4);
            new TileLocator().RadiusDistance(12, 16, 1, 12, 13, 1).Should().Be(4);

            new TileLocator().RadiusDistance(13, 15, 1, 13, 14, 1).Should().Be(1);
            new TileLocator().RadiusDistance(13, 15, 1, 14, 14, 1).Should().Be(1);
            new TileLocator().RadiusDistance(13, 15, 1, 14, 16, 1).Should().Be(1);
            new TileLocator().RadiusDistance(13, 15, 1, 13, 16, 1).Should().Be(1);

            new TileLocator().RadiusDistance(13, 15, 1, 13, 12, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 1, 12, 14, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 1, 14, 12, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 1, 15, 14, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 1, 12, 16, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 1, 13, 18, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 1, 14, 18, 1).Should().Be(4);
            new TileLocator().RadiusDistance(13, 15, 1, 15, 16, 1).Should().Be(4);

            new TileLocator().RadiusDistance(13, 15, 1, 16, 14, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 15, 1, 14, 20, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 15, 1, 13, 20, 1).Should().Be(7);
            new TileLocator().RadiusDistance(13, 15, 1, 11, 14, 1).Should().Be(7);
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
            new TileLocator().RadiusDistance(x1, y1, 1, x2, y2, 1).Should().Be(distance);
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
            new TileLocator().IsOverlapping(new Position(x1, y1), 1, r1, new Position(x2, y2), 1, r2)
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
            cacheForTestIsOverlappingWithCache.IsOverlapping(new Position(x1, y1), 1, r1, new Position(x2, y2), 1, r2)
                                              .Should()
                                              .Be(overlapping);
        }

        [Fact]
        public void TestIsOverlappingWithCache_WhenNotClose_ShouldNotHitCache()
        {
            var tileLocator = new TileLocator();

            tileLocator.IsOverlapping(new Position(50, 50), 1, 1, new Position(50, 47), 1, 0);

            tileLocator.OverlappingCache.Should().HaveCount(0);
        }

        [Theory]
        [InlineAutoNSubstituteData(50, 50, 2, 1, 49, 50, 1, 0, false)]
        [InlineAutoNSubstituteData(50, 50, 2, 1, 49, 50, 1, 1, true)]
        [InlineAutoNSubstituteData(50, 50, 2, 0, 51, 50, 1, 0, true)]
        [InlineAutoNSubstituteData(50, 50, 2, 0, 50, 48, 2, 0, true)]
        public void TestIsOverlapping_WithSize_ShouldOverlap(int x1, int y1, int size1, int r1, int x2, int y2, int size2, int r2, bool expected, TileLocator tileLocator)
        {
            var isOverlapping = tileLocator.IsOverlapping(new Position((uint)x1, (uint)y1), (byte)size1, (byte)r1, new Position((uint)x2, (uint)y2), (byte)size2, (byte)r2);

            isOverlapping.Should().Be(expected);
        }
    }
}