#region

using System.Collections.Generic;
using Game.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Testing.MapMath
{
    /// <summary>
    ///   Summary description for ChannelTest
    /// </summary>
    [TestClass]
    public class ForeachTest : TestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestEven()
        {
            var validPoints = new List<Point>();

            TileLocator.ForeachObject(10,
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

            ReverseTileLocator.ForeachObject(10,
                                              10,
                                              2,
                                              true,
                                              delegate(uint origX, uint origY, uint x, uint y, object custom)
                                                  {
                                                      ((List<Point>)custom).Add(new Point(x, y));
                                                      return true;
                                                  },
                                              reversePoints);

            CollectionAssert.AreEquivalent(validPoints, reversePoints);
        }

        [TestMethod]
        public void TestOdd()
        {
            var validPoints = new List<Point>();

            TileLocator.ForeachObject(10,
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

            ReverseTileLocator.ForeachObject(10,
                                              11,
                                              2,
                                              true,
                                              delegate(uint origX, uint origY, uint x, uint y, object custom)
                                                  {
                                                      ((List<Point>)custom).Add(new Point(x, y));
                                                      return true;
                                                  },
                                              reversePoints);

            CollectionAssert.AreEquivalent(validPoints, reversePoints);
        }

        #region Nested type: Point

        private class Point
        {
            public Point(uint x, uint y)
            {
                X = x;
                Y = y;
            }

            public uint X { get; set; }
            public uint Y { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != typeof(Point))
                    return false;
                return Equals((Point)obj);
            }

            private bool Equals(Point other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return other.X == X && other.Y == Y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X.GetHashCode()*397) ^ Y.GetHashCode();
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