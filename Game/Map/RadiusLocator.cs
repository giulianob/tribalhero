using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Game.Map
{
    public class RadiusLocator
    {
        private readonly ConcurrentDictionary<RadiusCache, bool> overlappingCache =
                new ConcurrentDictionary<RadiusCache, bool>();

        [Obsolete("Inject RadiusLocator instead")]
        public static RadiusLocator Current { get; set; }

        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        #endregion

        #region RadiusCache Item

        private class RadiusCache : IEquatable<RadiusCache>
        {
            public RadiusCache(int relativeX, int relativeY, byte radius, byte originalRadius)
            {
                RelativeX = relativeX;
                RelativeY = relativeY;
                Radius = radius;
                OriginalRadius = originalRadius;
            }

            public int RelativeX { get; private set; }

            public int RelativeY { get; private set; }

            public byte Radius { get; private set; }

            public byte OriginalRadius { get; private set; }

            public bool Equals(RadiusCache other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }
                if (ReferenceEquals(this, other))
                {
                    return true;
                }
                return other.RelativeX == RelativeX && other.RelativeY == RelativeY && other.Radius == Radius &&
                       other.OriginalRadius == OriginalRadius;
            }

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
                if (obj.GetType() != typeof(RadiusCache))
                {
                    return false;
                }
                return Equals((RadiusCache)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = RelativeX;
                    result = (result * 397) ^ RelativeY;
                    result = (result * 397) ^ Radius.GetHashCode();
                    result = (result * 397) ^ OriginalRadius.GetHashCode();
                    return result;
                }
            }
        }

        #endregion

        public virtual void ForeachObject(uint ox, uint oy, byte radius, bool doSelf, DoWork work, object custom)
        {
            TileLocator.Current.ForeachObject(ox,
                                              oy,
                                              radius,
                                              doSelf,
                                              (x, y, u, u1, c) =>
                                              RadiusDistance(x, y, u, u1) > radius || work(x, y, u, u1, custom));
        }

        public virtual int RadiusDistance(uint x, uint y, uint x1, uint y1)
        {
            // Calculate the x and y distances
            int offset = 0;
            int xoffset = 0;
            if (y % 2 != y1 % 2)
            {
                if (y % 2 == 0)
                {
                    if (x > x1)
                    {
                        xoffset = 1;
                    }
                }
                else
                {
                    if (x1 > x)
                    {
                        xoffset = 1;
                    }
                }
                offset = 1;
            }

            int xDistance = (int)(x > x1 ? x - x1 : x1 - x) - xoffset;
            var yDistance = (int)(y > y1 ? y - y1 : y1 - y);
            int yhalf = yDistance / 2;
            int x05 = Math.Min(xDistance, yhalf);
            int x15 = xDistance > yhalf ? xDistance - yhalf : 0;
            float radius = x05 * 0.5f + x15 * 1.5f + yhalf * 1.5f + offset;

            return Math.Max(0, (int)(radius * 2) - 1);
        }

        public virtual bool IsOverlapping(Position location1, byte r1, Position location2, byte r2)
        {
            var distance = RadiusDistance(location1.X, location1.Y, location2.X, location2.Y);

            var sumEqualToDistance = distance - 1 == r1 + r2;

            if (!sumEqualToDistance)
            {
                return distance - 1 < r1 + r2;
            }

            RadiusCache cacheItem = new RadiusCache((int)(location1.X - location2.X),
                                                    (int)(location1.Y - location2.Y),
                                                    r1,
                                                    r2);

            bool overlapping;

            if (overlappingCache.TryGetValue(cacheItem, out overlapping))
            {
                return overlapping;
            }

            HashSet<Position> points = new HashSet<Position>();

            ForeachObject(location1.X,
                          location1.Y,
                          r1,
                          true,
                          (x, y, x2, y2, custom) =>
                              {
                                  points.Add(new Position(x2, y2));

                                  return true;
                              },
                          null);

            ForeachObject(location2.X,
                          location2.Y,
                          r2,
                          true,
                          (x, y, x2, y2, custom) =>
                              {
                                  if (points.Contains(new Position(x2, y2)))
                                  {
                                      overlapping = true;
                                      return false;
                                  }

                                  return true;
                              },
                          null);

            overlappingCache.TryAdd(cacheItem, overlapping);

            return overlapping;
        }
    }
}