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
        public void ForeachMultiple_WhenYIsEvenAndSizeIs1(ITileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(0, 2, 1).Should().Equal(new Position(0, 2));
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenYIsOddAndSizeIs1(ITileLocator tileLocator)
        {
            tileLocator.ForeachMultitile(0, 1, 1).Should().Equal(new Position(0, 1));
        }

        [Theory, AutoNSubstituteData]
        public void ForeachMultiple_WhenXAndYAreEvenAndSizeIs2(ITileLocator tileLocator)
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
        public void ForeachMultiple_WhenXIsOddAndYIsEvenAndSizeIs2(ITileLocator tileLocator)
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
        public void ForeachMultiple_WhenXIsOddAndYIsOddAndSizeIs2(ITileLocator tileLocator)
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
        public void ForeachMultiple_WhenXIsEvenAndYIsOddAndSizeIs2(ITileLocator tileLocator)
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
        public void ForeachMultiple_WhenXIsEvenAndYIsOddAndSizeIs3(ITileLocator tileLocator)
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
        public void ForeachMultiple_WhenXIsOddAndYIsOddAndSizeIs3(ITileLocator tileLocator)
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
        public void ForeachMultiple_WhenXIsOddAndYIsEvenAndSizeIs3(ITileLocator tileLocator)
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
        public void ForeachMultiple_WhenXIsEvenAndYIsEvenAndSizeIs3(ITileLocator tileLocator)
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
    }
}