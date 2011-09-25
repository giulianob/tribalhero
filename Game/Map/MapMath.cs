#region

using System;

#endregion

namespace Game.Map
{
    public class Location : IEquatable<Location>
    {
        public Location(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public uint X { get; set; }
        public uint Y { get; set; }

        #region IEquatable<Location> Members

        bool IEquatable<Location>.Equals(Location other)
        {
            return X == other.X && Y == other.Y;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Location))
                return false;
            return Equals((Location)obj);
        }

        public bool Equals(Location other)
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
    }

    public class MapMath
    {
        public static uint AbsDiff(uint val1, uint val2)
        {
            return (val1 > val2 ? val1 - val2 : val2 - val1);
        }
    }

    public class TileLocator
    {
        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        #endregion

        private static readonly Random rand = new Random();

        public static void RandomPoint(uint ox, uint oy, byte radius, bool doSelf, out uint x, out uint y)
        {
            byte mode;
            if (ox%2 == 0)            
                mode = (byte)(oy%2 == 0 ? 0 : 1);            
            else            
                mode = (byte)(oy%2 == 0 ? 0 : 1);            

            do
            {
                uint cx = ox;
                uint cy = oy - (uint)(2*radius);

                var row = (byte)rand.Next(0, radius*2 + 1);
                var count = (byte)rand.Next(0, radius*2 + 1);

                for (int i = 0; i < row; i++)
                {
                    if (mode == 0)
                        cx -= (uint)((i + 1)%2);
                    else
                        cx -= (uint)((i)%2);

                    cy++;
                }

                if (row%2 == 0)
                {
                    if (mode == 0)
                    {
                        x = cx + (uint)((count)/2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count + 1)/2);
                        y = cy + count;
                    }
                }
                else
                {
                    // alternate row
                    if (mode == 0)
                    {
                        x = cx + (uint)((count + 1)/2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count)/2);
                        y = cy + count;
                    }
                }
            } while (!doSelf && (x == ox && y == oy));
        }

        public static void ForeachObject(uint ox, uint oy, byte radius, bool doSelf, DoWork work, object custom)
        {
            byte mode;
            if (ox%2 == 0)
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            //     Console.Out.WriteLine("offset:" + mode);
            uint cx = ox;
            uint cy = oy - (uint)(2*radius);
            bool done = false;
            for (byte row = 0; row < radius*2 + 1; ++row)
            {
                for (byte count = 0; count < radius*2 + 1; ++count)
                {
                    if (row%2 == 0)
                    {
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count)/2), cy + count, custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1)/2), cy + count, custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1)/2), cy + count, custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count)/2), cy + count, custom);
                        }
                    }

                    if (done)
                        break;
                }

                if (done)
                    break;

                if (mode == 0)
                {
                    cx -= (uint)((row + 1)%2);
                    //     Console.Out.WriteLine("cx:" + cx);
                }
                else
                {
                    cx -= (uint)((row)%2);
                    //  Console.Out.WriteLine("cx:" + cx);
                }

                ++cy;
            }
        }
    }

    public class ReverseTileLocator
    {
        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        #endregion

        private static readonly Random rand = new Random();

        public static void RandomPoint(uint ox, uint oy, byte radius, bool doSelf, out uint x, out uint y)
        {
            byte mode;
            if (ox%2 == 0)
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }

            do
            {
                uint cx = ox;
                uint cy = oy - (uint)(2*radius);

                var row = (byte)rand.Next(0, radius*2 + 1);
                var count = (byte)rand.Next(0, radius*2 + 1);

                for (int i = 0; i < row; i++)
                {
                    if (mode == 0)
                        cx -= (uint)((i + 1)%2);
                    else
                        cx -= (uint)((i)%2);

                    cy++;
                }

                if (row%2 == 0)
                {
                    if (mode == 0)
                    {
                        x = cx + (uint)((count)/2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count + 1)/2);
                        y = cy + count;
                    }
                }
                else
                {
                    // alternate row
                    if (mode == 0)
                    {
                        x = cx + (uint)((count + 1)/2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count)/2);
                        y = cy + count;
                    }
                }
            } while (!doSelf && (x == ox && y == oy));
        }

        public static void ForeachObject(uint ox, uint oy, byte radius, bool doSelf, DoWork work, object custom)
        {
            var mode = (byte)(oy%2 == 0 ? 0 : 1);

            //     Console.Out.WriteLine("offset:" + mode);
            uint cx = ox;
            uint cy = oy - (uint)(2*radius);

            bool done = false;
            for (byte row = 0; row < radius*2 + 1; ++row)
            {
                for (int count = radius*2; count >= 0; --count)
                {
                    if (row%2 == 0)
                    {
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count)/2), (uint)(cy + count), custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1)/2), (uint)(cy + count), custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1)/2), (uint)(cy + count), custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count)/2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count)/2), (uint)(cy + count), custom);
                        }
                    }

                    if (done)
                        break;
                }

                if (done)
                    break;

                if (mode == 0)
                {
                    cx -= (uint)((row + 1)%2);
                    //     Console.Out.WriteLine("cx:" + cx);
                }
                else
                {
                    cx -= (uint)((row)%2);
                    //  Console.Out.WriteLine("cx:" + cx);
                }

                ++cy;
            }
        }
    }

    public class RadiusLocator
    {
        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        #endregion

        private static readonly Random rand = new Random();

        public static void RandomPoint(uint ox, uint oy, byte radius, bool doSelf, out uint x, out uint y)
        {
            byte mode;
            if (ox%2 == 0)
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }

            do
            {
                uint cx = ox;
                uint cy = oy - (uint)(2*radius);

                var row = (byte)rand.Next(0, radius*2 + 1);
                var count = (byte)rand.Next(0, radius*2 + 1);

                for (int i = 0; i < row; i++)
                {
                    if (mode == 0)
                        cx -= (uint)((i + 1)%2);
                    else
                        cx -= (uint)((i)%2);

                    cy++;
                }

                if (row%2 == 0)
                {
                    if (mode == 0)
                    {
                        x = cx + (uint)((count)/2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count + 1)/2);
                        y = cy + count;
                    }
                }
                else
                {
                    // alternate row
                    if (mode == 0)
                    {
                        x = cx + (uint)((count + 1)/2);
                        y = cy + count;
                    }
                    else
                    {
                        x = cx + (uint)((count)/2);
                        y = cy + count;
                    }
                }
            } while (!doSelf && (x == ox && y == oy));
        }

        public static void ForeachObject(uint ox, uint oy, byte radius, bool doSelf, DoWork work, object custom)
        {
            byte mode;
            if (ox%2 == 0)
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy%2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            //     Console.Out.WriteLine("offset:" + mode);
            uint cx = ox;
            uint cy = oy - (uint)(2*radius);
            for (byte row = 0; row < radius*2 + 1; ++row)
            {
                for (byte count = 0; count < radius*2 + 1; ++count)
                {
                    if (row%2 == 0)
                    {
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count)/2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count)/2), cy + count, custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1)/2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count + 1)/2), cy + count, custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!doSelf && ox == cx + (uint)((count + 1)/2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count + 1)/2), cy + count, custom);
                        }
                        else
                        {
                            if (!doSelf && ox == cx + (uint)((count)/2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count)/2), cy + count, custom);
                        }
                    }
                }
                if (mode == 0)
                {
                    cx -= (uint)((row + 1)%2);
                    //     Console.Out.WriteLine("cx:" + cx);
                }
                else
                {
                    cx -= (uint)((row)%2);
                    //  Console.Out.WriteLine("cx:" + cx);
                }

                ++cy;
            }
        }
    }
}