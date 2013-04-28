#region

using FluentAssertions;
using Xunit;

#endregion

namespace Testing.CityTests
{
    /// <summary>
    ///     Summary description for ChannelTest
    /// </summary>
    public class CityTest
    {
        [Fact]
        public void TestValidName()
        {
            Game.Data.City.IsNameValid("Ab 1c d1 2").Should().BeTrue();
        }

        [Fact]
        public void TestLowerCase()
        {
            Game.Data.City.IsNameValid("ab 1c d1 2").Should().BeTrue();
        }

        [Fact]
        public void TestStartWithNumber()
        {
            Game.Data.City.IsNameValid("1Hello World").Should().BeFalse();
        }

        [Fact]
        public void TestTooShort()
        {
            Game.Data.City.IsNameValid("a").Should().BeFalse();
        }

        [Fact]
        public void TestJustShortEnough()
        {
            Game.Data.City.IsNameValid("xxx").Should().BeTrue();
        }

        [Fact]
        public void TestTooLong()
        {
            Game.Data.City.IsNameValid("xxxxxxxxxxxxxxxxx").Should().BeFalse();
        }

        [Fact]
        public void TestJustLongEnough()
        {
            Game.Data.City.IsNameValid("xxxxxxxxxxxxxxxx").Should().BeTrue();
        }

        [Fact]
        public void TestNewLine()
        {
            Game.Data.City.IsNameValid("xx\nxxx").Should().BeFalse();
        }
    }
}