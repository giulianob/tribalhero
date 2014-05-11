#region

using System.Collections.Generic;
using FluentAssertions;
using Game.Util;
using Xunit;

#endregion

namespace Testing.UtilTests
{
    /// <summary>
    ///     Summary description for ChannelTest
    /// </summary>
    public class XmlSerializerTest
    {
        [Fact]
        public void TestDeserializeComplexEmpty()
        {
            List<object[]> list = XmlSerializer.DeserializeComplexList("<List />");

            list.Count.Should().Be(0);
        }

        [Fact]
        public void TestDeserializeComplexEmptyProperty()
        {
            List<object[]> list = XmlSerializer.DeserializeComplexList("<List><Properties /></List>");

            list.Count.Should().Be(1);
            list[0].Length.Should().Be(0);
        }

        [Fact]
        public void TestDeserializeComplexEmptyPropertyMultiple()
        {
            List<object[]> list = XmlSerializer.DeserializeComplexList("<List><Properties /><Properties /></List>");

            list.Count.Should().Be(2);
            list[0].Length.Should().Be(0);
            list[1].Length.Should().Be(0);
        }
    }
}