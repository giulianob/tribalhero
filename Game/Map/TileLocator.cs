using System;
using System.Collections.Generic;

namespace Game.Map
{
    public class TileLocator
    {
        private readonly GetRandom getRandom;

        public TileLocator()
        {
        }

        public TileLocator(GetRandom getRandom)
        {
            this.getRandom = getRandom;
        }

        [Obsolete("Inject TileLocator instead")]
        public static TileLocator Current { get; set; }

        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        public delegate int GetRandom(int min, int max);

        #endregion

        private int GetOffset(uint x, uint y, uint x1, uint y1)
        {
            if (y % 2 == 1 && y1 % 2 == 0 && x1 <= x)
            {
                return 1;
            }

            if (y % 2 == 0 && y1 % 2 == 1 && x1 >= x)
            {
                return 1;
            }

            return 0;
        }

        public virtual int TileDistance(uint x, uint y, uint x1, uint y1)
        {
            int offset = GetOffset(x, y, x1, y1);
            var dist = (int)((x1 > x ? x1 - x : x - x1) + (y1 > y ? y1 - y : y - y1) / 2 + offset);

            return dist;
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

        public virtual void ForeachObject(uint ox, uint oy, int radius, bool doSelf, DoWork work, object custom = null)
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
                            if (!doSelf && ox == cx + (count) / 2 && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (count) / 2, cy + count, custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (count + 1) / 2 && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (count + 1) / 2, cy + count, custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (count + 1) / 2 && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (count + 1) / 2, cy + count, custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (count) / 2 && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (count) / 2, cy + count, custom);
                        }
                    }

                    if (done)
                    {
                        break;
                    }
                }

                if (done)
                {
                    break;
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
    }
}