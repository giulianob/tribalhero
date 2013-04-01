#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Database;
using Game.Map;

#endregion

namespace Game.Setup
{
    public class MapFactory
    {
        private const int SKIP = 1;

        private readonly List<Position> dict = new List<Position>();

        private int index;

        public void Init(string filename)
        {
            using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    String[] strs = line.Split(',');
                    uint x = uint.Parse(strs[0]);
                    uint y = uint.Parse(strs[1]);

                    dict.Add(new Position(x, y));
                }
            }
        }

        public IEnumerable<Position> Locations()
        {
            return dict;
        }

        public bool NextLocation(out uint x, out uint y, byte radius)
        {
            x = 0;
            y = 0;

            SystemVariable mapStartIndex;
            if (Global.Current.SystemVariables.TryGetValue("Map.start_index", out mapStartIndex) && index == 0)
            {
                index = (int)mapStartIndex.Value;
            }

            do
            {
                if (index >= dict.Count)
                {
                    return false;
                }

                Position point = dict[index];
                index += SKIP;

                // Check if objects already on that point
                List<ISimpleGameObject> objects = World.Current.GetObjects(point.X, point.Y);

                if (objects == null)
                {
                    continue;
                }

                if (objects.Count != 0)
                {
                    continue;
                }

                if (ForestManager.HasForestNear(point.X, point.Y, radius))
                {
                    continue;
                }

                x = point.X;
                y = point.Y;

                break;
            }
            while (true);

            if (mapStartIndex != null)
            {
                mapStartIndex.Value = index;
                DbPersistance.Current.Save(mapStartIndex);
            }

            return true;
        }
    }
}