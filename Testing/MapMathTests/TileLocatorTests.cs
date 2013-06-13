#region

using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Map;
using Xunit;
using Xunit.Extensions;

#endregion

namespace Testing.MapMathTests
{
    public class TileLocatorTests
    {

        #region ForeachMultiple

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenYIsEvenAndSizeIs1(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(0, 2, 1).Should().Equal(new Position(0, 2));
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenYIsOddAndSizeIs1(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(0, 1, 1).Should().Equal(new Position(0, 1));
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXAndYAreEvenAndSizeIs2(TileLocator tileLocator)
        {
            var positions = tileLocator.ForeachMultitile(50, 46, 2);

            positions.Should().HaveCount(4).
                      And.Contain(new[] { 
                                new Position(50, 46),
                                new Position(50, 45),
                                new Position(51, 46),
                                new Position(50, 47)
                      });
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXIsOddAndYIsEvenAndSizeIs2(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(49, 46, 2).Should().HaveCount(4).
                        And.Contain(new[]
                        {
                                new Position(49, 46),
                                new Position(49, 45),
                                new Position(50, 46),
                                new Position(49, 47)
                        }
                    );
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXIsOddAndYIsOddAndSizeIs2(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(49, 47, 2).Should().HaveCount(4).
                        And.Contain(new[]
                        {
                                new Position(49, 47),
                                new Position(50, 46),
                                new Position(50, 47),
                                new Position(50, 48)
                        }
                    );
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXIsEvenAndYIsOddAndSizeIs2(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(48, 47, 2).Should().HaveCount(4).
                        And.Contain(new[]
                        {
                                new Position(48, 47),
                                new Position(49, 46),
                                new Position(49, 47),
                                new Position(49, 48)
                        }
                    );
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXIsEvenAndYIsOddAndSizeIs3(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(48, 47, 3).Should().HaveCount(9).
                        And.Contain(new[]
                        {
                                new Position(48, 47),
                                new Position(49, 46),
                                new Position(49, 45),
                                new Position(49, 48),
                                new Position(49, 47),
                                new Position(50, 46),
                                new Position(49, 49),
                                new Position(50, 48),
                                new Position(50, 47),
                        }
                    );
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXIsOddAndYIsOddAndSizeIs3(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(49, 47, 3).Should().HaveCount(9).
                        And.Contain(new[]
                        {
                                new Position(49, 47),
                                new Position(50, 46),
                                new Position(50, 45),
                                new Position(50, 48),
                                new Position(50, 47),
                                new Position(51, 46),
                                new Position(50, 49),
                                new Position(51, 48),
                                new Position(51, 47),
                        }
                    );
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXIsOddAndYIsEvenAndSizeIs3(TileLocator tileLocator)
        {
            var positions = tileLocator.ForeachMultitile(49, 46, 3);

            positions.Should().HaveCount(9).
                        And.Contain(new[]
                        {
                                new Position(49, 46),
                                new Position(49, 45),
                                new Position(50, 44),
                                new Position(49, 47),
                                new Position(50, 46),
                                new Position(50, 45),
                                new Position(50, 48),
                                new Position(50, 47),
                                new Position(51, 46),
                        }
                    );
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXIsEvenAndYIsEvenAndSizeIs3(TileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(50, 46, 3).Should().HaveCount(9).
                        And.Contain(new[]
                        {
                                new Position(50, 46),
                                new Position(50, 45),
                                new Position(51, 44),
                                new Position(50, 47),
                                new Position(51, 46),
                                new Position(51, 45),
                                new Position(51, 48),
                                new Position(51, 47),
                                new Position(52, 46),
                        }
                    );
        }

        #endregion

        #region ForEach

        [Fact]
        public void ForEach_WhenEven()
        {
            var validPoints = new List<Point>();

            new TileLocator().ForeachObject(10,
                                            10,
                                            2,
                                            true,
                                            delegate(uint origX, uint origY, uint x, uint y, object custom)
                                                {
                                                    ((List<Point>)custom).Add(new Point(x, y));
                                                    return true;
                                                },
                                            validPoints);

            var reversePoints = new List<Point>();

            new ReverseTileLocator().ForeachObject(10,
                                                   10,
                                                   2,
                                                   true,
                                                   delegate(uint origX, uint origY, uint x, uint y, object custom)
                                                       {
                                                           ((List<Point>)custom).Add(new Point(x, y));
                                                           return true;
                                                       },
                                                   reversePoints);

            validPoints.Should().BeEquivalentTo(reversePoints);
        }

        [Fact]
        public void ForEach_WhenOdd()
        {
            var validPoints = new List<Point>();

            new TileLocator().ForeachObject(10,
                                            11,
                                            2,
                                            true,
                                            delegate(uint origX, uint origY, uint x, uint y, object custom)
                                                {
                                                    ((List<Point>)custom).Add(new Point(x, y));
                                                    return true;
                                                },
                                            validPoints);

            var reversePoints = new List<Point>();

            new ReverseTileLocator().ForeachObject(10,
                                                   11,
                                                   2,
                                                   true,
                                                   delegate(uint origX, uint origY, uint x, uint y, object custom)
                                                       {
                                                           ((List<Point>)custom).Add(new Point(x, y));
                                                           return true;
                                                       },
                                                   reversePoints);

            validPoints.Should().BeEquivalentTo(reversePoints);
        }
        
        #endregion

        #region Nested type: Point

        private class Point
        {
            public Point(uint x, uint y)
            {
                X = x;
                Y = y;
            }

            private uint X { get; set; }

            private uint Y { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != typeof(Point))
                {
                    return false;
                }
                return Equals((Point)obj);
            }

            private bool Equals(Point other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }
                if (ReferenceEquals(this, other))
                {
                    return true;
                }
                return other.X == X && other.Y == Y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X.GetHashCode() * 397) ^ Y.GetHashCode();
                }
            }

            public override string ToString()
            {
                return string.Format("({0},{1})", X, Y);
            }
        }

        #endregion
    }
}