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

        class Point
        {
            public uint X { get; set; }
            public uint Y { get; set; }
            public Point(uint x, uint y)
            {
                X = x;
                Y = y;
            }
        }

        private static Dictionary<int, List<Point>> dict = new Dictionary<int, List<Point>>();
        private static int index;
        private static int region_index;
        private static uint region_width = Config.region_width * 2;
        private static uint region_height = Config.region_height * 3;
        private static int region_col = (int)(Config.map_width / region_width);
        private static int region_row = (int)(Config.map_height / region_height);
        private static int region_count = region_col * region_row;

        private static int GetRegion(uint x, uint y)
        {
            return (int)(x / region_width + (y / region_height) * region_col);
        }

        public static void init(string filename)
        {
            //     if (Config.map_width % region_width != 0 || Config.map_height % region_height != 0) throw new Exception();
            for (int i = 0; i < region_count; ++i)
                dict[i] = new List<Point>();
            using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
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
                        for (int i = region_count; i <= region; ++i)
                        {
                            list = new List<Point>();
                            dict[i] = list;
                        }
                        region_count = region + 1;
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
                if (!dict.TryGetValue(region_index, out list))
                {
                    return false;
                }

                do
                {
                    if (index >= list.Count)
                    {
                        index = 0;
                        ++region_index;
                        break;
                    }

                    Point point = list[index++];
                    List<SimpleGameObject> objects = Global.World.GetObjects(point.X, point.Y);

                    if (objects == null)
                        continue;

                    if (objects.Count == 0) {
                        x = point.X;
                        y = point.Y;
                        return true;
                    }

                } while (true);
            } while (true);
        }
    }
}