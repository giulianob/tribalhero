#region

using FluentAssertions;
using Game.Map;
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
            CityManager.IsNameValid("Ab 1c d1 2").Should().BeTrue();
        }

        [Fact]
        public void TestLowerCase()
        {
            CityManager.IsNameValid("ab 1c d1 2").Should().BeTrue();
        }

        [Fact]
        public void TestStartWithNumber()
        {
            CityManager.IsNameValid("1Hello World").Should().BeFalse();
        }

        [Fact]
        public void TestTooShort()
        {
            CityManager.IsNameValid("a").Should().BeFalse();
        }

        [Fact]
        public void TestJustShortEnough()
        {
            CityManager.IsNameValid("xxx").Should().BeTrue();
        }

        [Fact]
        public void TestTooLong()
        {
            CityManager.IsNameValid("xxxxxxxxxxxxxxxxx").Should().BeFalse();
        }

        [Fact]
        public void TestJustLongEnough()
        {
            CityManager.IsNameValid("xxxxxxxxxxxxxxxx").Should().BeTrue();
        }

        [Fact]
        public void TestNewLine()
        {
            CityManager.IsNameValid("xx\nxxx").Should().BeFalse();
        }
    }
}