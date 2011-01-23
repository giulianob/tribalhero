#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;

#endregion

namespace Game.Setup
{
    public class MapFactory
    {
        private static readonly Dictionary<int, List<Point>> dict = new Dictionary<int, List<Point>>();
        private static int index;
        private static int regionIndex;
        private static readonly uint regionWidth = Config.region_width*2;
        private static readonly uint regionHeight = Config.region_height*3;
        private static readonly int regionCol = (int)(Config.map_width/regionWidth);
        private static readonly int regionRow = (int)(Config.map_height/regionHeight);
        private static int regionCount = regionCol*regionRow;

        private static int GetRegion(uint x, uint y)
        {
            return (int)(x/regionWidth + (y/regionHeight)*regionCol);
        }

        public static void Init(string filename)
        {
            for (int i = 0; i < regionCount; ++i)
                dict[i] = new List<Point>();
            using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    String[] strs = line.Split(',');
                    uint x = uint.Parse(strs[0]);
                    uint y = uint.Parse(strs[1]);
                    int region = GetRegion(x, y);

                    List<Point> list;
                    if (!dict.TryGetValue(region, out list))
                    {
                        for (int i = regionCount; i <= region; ++i)
                        {
                            list = new List<Point>();
                            dict[i] = list;
                        }
                        regionCount = region + 1;
                    }

                    list.Add(new Point(x, y));
                }
            }
        }

        public static bool NextLocation(out uint x, out uint y)
        {
            x = 0;
            y = 0;

            List<Point> list;
            do
            {
                if (!dict.TryGetValue(regionIndex, out list))
                    return false;

                do
                {
                    if (index >= list.Count)
                    {
                        index = 0;
                        ++regionIndex;
                        break;
                    }

                    Point point = list[index++];
                    List<SimpleGameObject> objects = Global.World.GetObjects(point.X, point.Y);

                    if (objects == null)
                        continue;

                    if (objects.Count == 0)
                    {
                        x = point.X;
                        y = point.Y;
                        return true;
                    }
                } while (true);
            } while (true);
        }

        #region Nested type: Point

        private class Point
        {
            public Point(uint x, uint y)
            {
                X = x;
                Y = y;
            }

            public uint X { get; private set; }
            public uint Y { get; private set; }
        }

        #endregion
    }
}