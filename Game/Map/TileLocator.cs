using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Game.Util;

namespace Game.Map
{
    public class TileLocator
    {
        private readonly ConcurrentDictionary<RadiusCache, bool> overlappingCache = new ConcurrentDictionary<RadiusCache, bool>();

        private readonly GetRandom getRandom;

        [Obsolete("Inject TileLocator instead")]
        public static TileLocator Current { get; set; }

        public ConcurrentDictionary<RadiusCache, bool> OverlappingCache
        {
            get
            {
                return overlappingCache;
            }
        }

        public TileLocator()
        {            
        }

        public TileLocator(GetRandom getRandom)
        {
            this.getRandom = getRandom;
        }

        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        #endregion

        #region RadiusCache Item

        public class RadiusCache : IEquatable<RadiusCache>
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

            if (OverlappingCache.TryGetValue(cacheItem, out overlapping))
            {
                return overlapping;
            }

            HashSet<Position> points = new HashSet<Position>(ForeachRadius(location1.X, location1.Y, r1));

            if (ForeachRadius(location2.X, location2.Y, r2).Any(position => points.Contains(new Position(position.X, position.Y))))
            {
                overlapping = true;
            }

            OverlappingCache.TryAdd(cacheItem, overlapping);

            return overlapping;
        }

        public virtual int TileDistance(uint x, uint y, uint x1, uint y1)
        {
            var offset = 0;
            if ((y % 2 == 1 && y1 % 2 == 0 && x1 <= x) ||
                (y % 2 == 0 && y1 % 2 == 1 && x1 >= x))
            {
                offset = 1;
            }

            return (int)((x1 > x ? x1 - x : x - x1) + (y1 > y ? y1 - y : y - y1) / 2 + offset);
        }

        public virtual void RandomPoint(uint ox, uint oy, byte radius, bool doSelf, out uint x, out uint y)
        {
            byte mode = (byte)(oy % 2 == 0 ? 0 : 1);

            do
            {
                uint cx = ox;
                uint cy = oy - (uint)(2 * radius);

                var row = (byte)getRandom(0, radius * 2 + 1);
                var count = (byte)getRandom(0, radius * 2 + 1);

                for (int i = 0; i < row; i++)
                {
                    if (mode == 0)
                    {
                        cx -= (uint)((i + 1) % 2);
                    }
                    else
                    {
                        cx -= (uint)((i) % 2);
                    }

                    cy++;
                }

                if (row % 2 == 0)
                {
                    if (mode == 0)
                    {
                        x = cx + (uint)((count) / 2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count + 1) / 2);
                        y = cy + count;
                    }
                }
                else
                {
                    // alternate row
                    if (mode == 0)
                    {
                        x = cx + (uint)((count + 1) / 2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count) / 2);
                        y = cy + count;
                    }
                }
            }
            while (!doSelf && (x == ox && y == oy));
        }

        public virtual IEnumerable<Position> ForeachMultitile(uint ox, uint oy, byte size)
        {
            var position = new Position(ox, oy);
            for (uint i = 0; i < size; i++)
            {
                var rowPosition = position;
                for (uint j = 0; j < size; j++)
                {
                    yield return rowPosition;

                    rowPosition = rowPosition.TopRight();
                }

                position = position.BottomRight();
            }
        }

        public virtual IEnumerable<Position> ForeachTile(uint ox, uint oy, int radius, bool includeCenter  = true)
        {
            byte mode = (byte)(oy % 2 == 0 ? 0 : 1);

            uint cx = ox;
            uint cy = oy - (uint)(2 * radius);
            bool done = false;
            for (uint row = 0; row < radius * 2 + 1; ++row)
            {
                for (uint count = 0; count < radius * 2 + 1; ++count)
                {
                    if (row % 2 == 0)
                    {
                        if (mode == 0)
                        {
                            if (!includeCenter && ox == cx + (count) / 2 && oy == cy + count)
                            {
                                continue;
                            }

                            yield return new Position(cx + (count) / 2, cy + count);
                        }
                        else
                        {
                            if (!includeCenter && ox == cx + (count + 1) / 2 && oy == cy + count)
                            {
                                continue;
                            }

                            yield return new Position(cx + (count + 1) / 2, cy + count);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!includeCenter && ox == cx + (count + 1) / 2 && oy == cy + count)
                            {
                                continue;
                            }

                            yield return new Position(cx + (count + 1) / 2, cy + count);
                        }
                        else
                        {
                            if (!includeCenter && ox == cx + (count) / 2 && oy == cy + count)
                            {
                                continue;
                            }

                            yield return new Position(cx + (count) / 2, cy + count);
                        }
                    }
                }

                if (mode == 0)
                {
                    cx -= (row + 1) % 2;
                }
                else
                {
                    cx -= (row) % 2;
                }

                ++cy;
            }
        }

        public virtual IEnumerable<Position> ForeachRadius(uint ox, uint oy, byte radius, bool includeCenter = true)
        {
            return ForeachTile(ox, oy, radius, includeCenter).Where(position => RadiusDistance(ox, oy, position.X, position.Y) <= radius);
        }

    }
}