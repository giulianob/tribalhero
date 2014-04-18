#region

using FluentAssertions;
using Game.Map;
using Xunit.Extensions;

#endregion

namespace Testing.MapMathTests
{
    public class MapMathTests
    {
        [Theory, InlineData(13, 11, 11, 8, false), InlineData(13, 11, 11, 10, false), InlineData(13, 11, 11, 12, false),
         InlineData(13, 11, 12, 9, false), InlineData(13, 11, 12, 10, false), InlineData(13, 11, 13, 12, false),
         InlineData(13, 11, 14, 13, false), InlineData(13, 11, 14, 14, false), InlineData(13, 11, 15, 15, false),
         InlineData(13, 11, 13, 16, false), InlineData(13, 11, 12, 16, false), InlineData(13, 11, 12, 12, false),
         InlineData(13, 11, 13, 12, false), InlineData(13, 11, 13, 10, false), InlineData(13, 11, 14, 13, false),
         InlineData(13, 11, 14, 15, false), InlineData(13, 11, 13, 13, true), InlineData(13, 11, 13, 15, true),
         InlineData(13, 11, 12, 11, true), InlineData(13, 11, 13, 15, true), InlineData(13, 11, 11, 11, true),
         InlineData(13, 11, 13, 9, true), InlineData(13, 11, 13, 11, true), InlineData(13, 11, 13, 15, true),
         InlineData(13, 11, 15, 11, true), InlineData(13, 11, 14, 11, true), InlineData(12, 15, 10, 12, false),
         InlineData(12, 15, 11, 13, false), InlineData(12, 15, 12, 16, false), InlineData(12, 15, 13, 17, false),
         InlineData(12, 15, 13, 19, false), InlineData(12, 15, 13, 20, false), InlineData(12, 15, 10, 17, false),
         InlineData(13, 18, 12, 15, false), InlineData(13, 18, 12, 16, false), InlineData(13, 18, 13, 17, false),
         InlineData(1689, 3118, 1691, 3117, false)]
        public void TestPerpendicular(int x, int y, int x1, int y1, bool expected)
        {
            new MapMath().IsPerpendicular((uint)x, (uint)y, (uint)x1, (uint)y1).Should().Be(expected);
        }


    }
}