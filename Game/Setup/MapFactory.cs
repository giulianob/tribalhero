#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Database;
using Game.Map;
using Persistance;

#endregion

namespace Game.Setup
{
    public class MapFactory
    {
        private readonly IDbManager dbManager;
        private readonly ISystemVariableManager systemVariableManager;

        private const int INITIAL_INDEX = 200;

        private readonly List<Position> dict = new List<Position>();

        public MapFactory(IDbManager dbManager, ISystemVariableManager systemVariableManager)
        {
            this.dbManager = dbManager;
            this.systemVariableManager = systemVariableManager;
        }

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
                    if (!systemVariableManager.TryGetValue("Map.start_index", out index))
                    {
                        index = new SystemVariable("Map.start_index", INITIAL_INDEX);
                    }
                }

                return (int)index.Value;
            }
            set
            {
                index.Value = value;
                dbManager.Save(index);
            }
        }
    }
}