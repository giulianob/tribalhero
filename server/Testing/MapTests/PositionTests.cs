using Common.Testing;
using FluentAssertions;
using Game.Map;
using Xunit;
using Xunit.Extensions;

namespace Testing.MapTests
{
    public class PositionTests
    {
        [Fact]
        public void Equals_ReturnsTrueIfObjectsAreEqual()
        {
            new Position(50, 50).Equals(new Position(50, 50)).Should().BeTrue();
        }

        [Theory]
        [InlineAutoNSubstituteData(50, 46, 50, 44)]
        [InlineAutoNSubstituteData(49, 47, 49, 45)]
        public void Top_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).Top().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }

        [Theory]
        [InlineAutoNSubstituteData(50, 44, 50, 46)]
        [InlineAutoNSubstituteData(49, 45, 49, 47)]
        public void Bottom_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).Bottom().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }

        [Theory]
        [InlineAutoNSubstituteData(49, 50, 48, 50)]
        [InlineAutoNSubstituteData(49, 49, 48, 49)]
        public void Left_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).Left().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }

        [Theory]
        [InlineAutoNSubstituteData(48, 50, 49, 50)]
        [InlineAutoNSubstituteData(48, 49, 49, 49)]
        public void Right_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).Right().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }

        [Theory]
        [InlineAutoNSubstituteData(50, 48, 49, 47)]
        [InlineAutoNSubstituteData(50, 49, 50, 48)]
        public void TopLeft_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).TopLeft().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }

        [Theory]
        [InlineAutoNSubstituteData(51, 48, 51, 47)]
        [InlineAutoNSubstituteData(50, 47, 51, 46)]
        public void TopRight_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).TopRight().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }
        
        [Theory]
        [InlineAutoNSubstituteData(49, 50, 48, 51)]
        [InlineAutoNSubstituteData(49, 49, 49, 50)]
        public void BottomLeft_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).BottomLeft().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }

        [Theory]
        [InlineAutoNSubstituteData(51, 50, 51, 51)]
        [InlineAutoNSubstituteData(51, 47, 52, 48)]
        public void BottomRight_ReturnsCorrectPosition(int x, int y, int expectedX, int expectedY)
        {
            new Position((uint)x, (uint)y).BottomRight().Should().Be(new Position((uint)expectedX, (uint)expectedY));
        }
    }
}