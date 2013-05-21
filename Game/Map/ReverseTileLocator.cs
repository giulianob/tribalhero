using System;

namespace Game.Map
{
    public class ReverseTileLocator
    {
        private readonly GetRandom getRandom;

        public ReverseTileLocator()
        {
        }

        public ReverseTileLocator(GetRandom getRandom)
        {
            this.getRandom = getRandom;
        }

        [Obsolete("Inject ReverseTileLocator instead")]
        public static ReverseTileLocator Current { get; set; }

        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        public delegate int GetRandom(int min, int max);

        #endregion

        public virtual void RandomPoint(uint ox, uint oy, byte radius, bool doSelf, out uint x, out uint y)
        {
            var mode = (byte)(oy % 2 == 0 ? 0 : 1);

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

        public virtual void ForeachObject(uint ox, uint oy, byte radius, bool doSelf, DoWork work, object custom = null)
        {
            var mode = (byte)(oy % 2 == 0 ? 0 : 1);

            uint cx = ox;
            uint cy = oy - (uint)(2 * radius);

            bool done = false;
            for (byte row = 0; row < radius * 2 + 1; ++row)
            {
                for (int count = radius * 2; count >= 0; --count)
                {
                    if (row % 2 == 0)
                    {
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count) / 2) && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (uint)((count) / 2), (uint)(cy + count), custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (uint)((count + 1) / 2), (uint)(cy + count), custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (uint)((count + 1) / 2), (uint)(cy + count), custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count) / 2) && oy == cy + count)
                            {
                                continue;
                            }
                            done = !work(ox, oy, cx + (uint)((count) / 2), (uint)(cy + count), custom);
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
                    cx -= (uint)((row + 1) % 2);
                }
                else
                {
                    cx -= (uint)((row) % 2);
                }

                ++cy;
            }
        }
    }
}