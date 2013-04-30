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

        private SystemVariable index;
        public int Index
        {
            get
            {
                if (index == null)
                {
                    if (!Global.Current.SystemVariables.TryGetValue("Map.start_index", out index))
                    {
                        return 0;
                    }
                }
                return (int)index.Value;
            }
            set
            {
                index.Value = value;
                DbPersistance.Current.Save(index);
            }
        }
    }
}