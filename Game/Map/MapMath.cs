#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;

#endregion

namespace Game.Map
{
    public class Location : IEquatable<Location>
    {
        public uint x;
        public uint y;
        public Location(uint x, uint y)
        {
            this.x = x;
            this.y = y;
        }

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
            return other.x == x && other.y == y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

        bool IEquatable<Location>.Equals(Location other)
        {
            return x == other.x && y == other.y;
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
        private static Random rand = new Random();

        public delegate bool do_work(uint origX, uint origY, uint x, uint y, object custom);

        public TileLocator(uint x, uint y, byte radius) { }

        public static void random_point(uint ox, uint oy, byte radius, bool do_self, out uint x, out uint y)
        {
            byte mode;
            if (ox % 2 == 0)
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }

            do
            {
                uint cx = ox;
                uint cy = oy - (uint)(2 * radius);

                byte row = (byte)rand.Next(0, radius * 2 + 1);
                byte count = (byte)rand.Next(0, radius * 2 + 1);

                for (int i = 0; i < row; i++)
                {
                    if (mode == 0)
                        cx -= (uint)((i + 1) % 2);
                    else
                        cx -= (uint)((i) % 2);

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
            } while (!do_self && (x == ox && y == oy));
        }

        public static void foreach_object(uint ox, uint oy, byte radius, bool do_self, do_work work, object custom)
        {
            byte mode;
            if (ox % 2 == 0)
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            //     Console.Out.WriteLine("offset:" + mode);
            uint cx = ox;
            uint cy = oy - (uint)(2 * radius);
            uint last = cx;
            bool done = false;
            for (byte row = 0; row < radius * 2 + 1; ++row)
            {
                for (byte count = 0; count < radius * 2 + 1; ++count)
                {
                    if (row % 2 == 0)
                    {
                        if (mode == 0)
                        {
                            if (!do_self && ox == cx + (uint)((count) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count) / 2), cy + count, custom);
                        }
                        else
                        {
                            if (!do_self && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1) / 2), cy + count, custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!do_self && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1) / 2), cy + count, custom);
                        }
                        else
                        {
                            if (!do_self && ox == cx + (uint)((count) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count) / 2), cy + count, custom);
                        }
                    }

                    if (done) break;
                }

                if (done) break;

                if (mode == 0)
                {
                    cx -= (uint)((row + 1) % 2);
                    //     Console.Out.WriteLine("cx:" + cx);
                }
                else
                {
                    cx -= (uint)((row) % 2);
                    //  Console.Out.WriteLine("cx:" + cx);
                }

                ++cy;
            }
        }
    }

    public class ReverseTileLocator
    {
        private static Random rand = new Random();

        public delegate bool do_work(uint origX, uint origY, uint x, uint y, object custom);

        public ReverseTileLocator(uint x, uint y, byte radius) { }

        public static void random_point(uint ox, uint oy, byte radius, bool do_self, out uint x, out uint y)
        {
            byte mode;
            if (ox % 2 == 0)
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }

            do
            {
                uint cx = ox;
                uint cy = oy - (uint)(2 * radius);

                byte row = (byte)rand.Next(0, radius * 2 + 1);
                byte count = (byte)rand.Next(0, radius * 2 + 1);

                for (int i = 0; i < row; i++)
                {
                    if (mode == 0)
                        cx -= (uint)((i + 1) % 2);
                    else
                        cx -= (uint)((i) % 2);

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
            } while (!do_self && (x == ox && y == oy));
        }

        public static void foreach_object(uint ox, uint oy, byte radius, bool do_self, do_work work, object custom)
        {
            byte mode = (byte)(oy % 2 == 0 ? 0 : 1);

            //     Console.Out.WriteLine("offset:" + mode);
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
                            if (!do_self && ox == cx + (uint)((count) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count) / 2), (uint)(cy + count), custom);
                        }
                        else
                        {
                            if (!do_self && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1) / 2), (uint)(cy + count), custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!do_self && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count + 1) / 2), (uint)(cy + count), custom);
                        }
                        else
                        {
                            if (!do_self && ox == cx + (uint)((count) / 2) && oy == cy + count)
                                continue;
                            done = !work(ox, oy, cx + (uint)((count) / 2), (uint)(cy + count), custom);
                        }
                    }

                    if (done) break;
                }

                if (done) break;

                if (mode == 0)
                {
                    cx -= (uint)((row + 1) % 2);
                    //     Console.Out.WriteLine("cx:" + cx);
                }
                else
                {
                    cx -= (uint)((row) % 2);
                    //  Console.Out.WriteLine("cx:" + cx);
                }

                ++cy;
            }
        }
    }

    public class RadiusLocator
    {
        private static Random rand = new Random();

        public delegate bool do_work(uint origX, uint origY, uint x, uint y, object custom);

        public RadiusLocator(uint x, uint y, byte radius) { }

        public static void random_point(uint ox, uint oy, byte radius, bool do_self, out uint x, out uint y)
        {
            byte mode;
            if (ox % 2 == 0)
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }

            do
            {
                uint cx = ox;
                uint cy = oy - (uint)(2 * radius);

                byte row = (byte)rand.Next(0, radius * 2 + 1);
                byte count = (byte)rand.Next(0, radius * 2 + 1);

                for (int i = 0; i < row; i++)
                {
                    if (mode == 0)
                        cx -= (uint)((i + 1) % 2);
                    else
                        cx -= (uint)((i) % 2);

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
            } while (!do_self && (x == ox && y == oy));
        }

        public static void foreach_object(uint ox, uint oy, byte radius, bool do_self, do_work work, object custom)
        {
            byte mode;
            if (ox % 2 == 0)
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            else
            {
                if (oy % 2 == 0)
                    mode = 0;
                else
                    mode = 1;
            }
            //     Console.Out.WriteLine("offset:" + mode);
            uint cx = ox;
            uint cy = oy - (uint)(2 * radius);
            uint last = cx;
            for (byte row = 0; row < radius * 2 + 1; ++row)
            {
                for (byte count = 0; count < radius * 2 + 1; ++count)
                {
                    if (row % 2 == 0)
                    {
                        if (mode == 0)
                        {
                            if (!do_self && ox == cx + (uint)((count) / 2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count) / 2), cy + count, custom);
                        }
                        else
                        {
                            if (!do_self && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count + 1) / 2), cy + count, custom);
                        }
                    }
                    else
                    {
                        // alternate row
                        if (mode == 0)
                        {
                            if (!do_self && ox == cx + (uint)((count + 1) / 2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count + 1) / 2), cy + count, custom);
                        }
                        else
                        {
                            if (!do_self && ox == cx + (uint)((count) / 2) && oy == cy + count)
                                continue;
                            work(ox, oy, cx + (uint)((count) / 2), cy + count, custom);
                        }
                    }
                }
                if (mode == 0)
                {
                    cx -= (uint)((row + 1) % 2);
                    //     Console.Out.WriteLine("cx:" + cx);
                }
                else
                {
                    cx -= (uint)((row) % 2);
                    //  Console.Out.WriteLine("cx:" + cx);
                }

                ++cy;
            }
        }
    }

    public class RoadPathFinder
    {
        delegate List<Location> GetNeighbors(Location node);        

        public static bool HasPath(Location start, Location end, City city, Location excludedPoint)
        {
            bool fromStructure = Global.World[start.x, start.y].Exists(s => s is Structure);

            return BreadthFirst(new Location(end.x, end.y), new List<Location> { new Location(start.x, start.y) }, excludedPoint, delegate(Location node)
            {
                List<Location> neighbors = new List<Location>(4);
                Location[] possibleNeighbors;
                if (node.y % 2 == 0)
                {
                    possibleNeighbors = new[]{
                                                                                                                                   new Location(node.x, node.y - 1), new Location(node.x, node.y + 1),
                                                                                                                                   new Location(node.x - 1, node.y - 1),
                                                                                                                                   new Location(node.x - 1, node.y + 1)
                                                                                                                               };
                }
                else
                {
                    possibleNeighbors = new[]{
                                                                                                                                   new Location(node.x + 1, node.y - 1),
                                                                                                                                   new Location(node.x + 1, node.y + 1), new Location(node.x, node.y - 1),
                                                                                                                                   new Location(node.x, node.y + 1)
                                                                                                                               };
                }

                neighbors.AddRange(
                    possibleNeighbors.Where(
                        delegate(Location location) {
                            if (!location.Equals(end)) {
                                if (Global.World[location.x, location.y].Exists(s => s is Structure)) return false;
                                if (!RoadManager.IsRoad(location.x, location.y)) return false;
                                if (SimpleGameObject.TileDistance(location.x, location.y, city.MainBuilding.X, city.MainBuilding.Y) > city.Radius) return false;
                            } else if (fromStructure && node.Equals(start)) {
                                return false;
                            }

                            return true;
                        }));

                return neighbors;
            });
        }

        private static bool BreadthFirst(Location end, List<Location> visited, Location excludedPoint, GetNeighbors getNeighbors) {
            List<Location> nodes = getNeighbors(visited.Last());

            // Examine adjacent nodes for end goal
            if (nodes.Contains(end))
                return true;

            // in breadth-first, recursion needs to come after visiting adjacent nodes
            foreach (Location node in nodes.Where(node => !node.Equals(end) && !node.Equals(excludedPoint) && !visited.Contains(node))) {
                visited.Add(node);
                if (BreadthFirst(end, visited, excludedPoint, getNeighbors))
                    return true;

                visited.RemoveAt(visited.Count - 1);
            }

            return false;
        }

        /*
         * This is the normal breadth first in case we need it one day
        private static void BreadthFirst(Location end, List<Location> visited, List<List<Location>> paths, GetNeighbors getNeighbors)
        {
            List<Location> nodes = getNeighbors(visited.Last());
            // Examine adjacent nodes for end goal
            foreach (Location node in nodes.Where(node => node.Equals(end) && !visited.Contains(node))) {
                visited.Add(node);
                paths.Add(new List<Location>(visited));
                visited.RemoveAt(visited.Count - 1);
                break;
            }

            // in breadth-first, recursion needs to come after visiting adjacent nodes
            foreach (Location node in nodes.Where(node => !node.Equals(end) && !visited.Contains(node))) {
                visited.Add(node);
                BreadthFirst(end, visited, paths, getNeighbors);
                visited.RemoveAt(visited.Count - 1);
            }
        }
         */
    }
}