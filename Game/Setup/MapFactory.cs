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
        private static readonly List<Point> dict = new List<Point>();
        private static int index;
        private const int SKIP = 2; 

        public static void Init(string filename)
        {
            using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    String[] strs = line.Split(',');
                    uint x = uint.Parse(strs[0]);
                    uint y = uint.Parse(strs[1]);

                    dict.Add(new Point(x, y));
                }
            }
        }

        public static bool NextLocation(out uint x, out uint y)
        {
            x = 0;
            y = 0;

            do {
                if(index>=dict.Count)
                {
                    return false;
                }
                Point point = dict[index];
                index += SKIP;
                List<SimpleGameObject> objects = Global.World.GetObjects(point.X, point.Y);

                if (objects == null)
                    continue;

                if (objects.Count == 0) {
                    x = point.X;
                    y = point.Y;
                    return true;
                }

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