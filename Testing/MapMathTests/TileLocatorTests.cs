#region

using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Map;
using NSubstitute;
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

        [Theory]
        [InlineAutoNSubstituteData(50, 50, 1, 51, 48, 2, 3)]
        [InlineAutoNSubstituteData(51, 48, 2, 50, 50, 1, 3)]
        [InlineAutoNSubstituteData(48, 50, 3, 51, 49, 2, 4)]
        public void RadiusDistance_WithObj_ShouldReturnLowestRadiusDistanceBetweenObjTiles(
            int x, int y, int size,
            int x1, int y1, int size1,
            int expected,
            ISimpleGameObject obj1,
            ISimpleGameObject obj2,
            TileLocator tileLocator
            )
        {
            obj1.PrimaryPosition.X.Returns((uint)x);
            obj1.PrimaryPosition.Y.Returns((uint)y);
            obj1.Size.Returns((byte)size);

            obj2.PrimaryPosition.X.Returns((uint)x1);
            obj2.PrimaryPosition.Y.Returns((uint)y1);
            obj2.Size.Returns((byte)size1);

            tileLocator.RadiusDistance(obj1, obj2).Should().Be(expected);
        }

        #endregion
    }
}