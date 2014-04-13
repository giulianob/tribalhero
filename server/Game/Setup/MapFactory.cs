#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Map;
using Persistance;

#endregion

namespace Game.Setup
{
    public class MapFactory
    {
        private const int MIN_DISTANCE_AWAY_FROM_CITIES = 9;

        private readonly IDbManager dbManager;

        private readonly ISystemVariableManager systemVariableManager;

        private readonly ITileLocator tileLocator;

        private const int INITIAL_INDEX = 200;

        private readonly List<Position> cityLocations = new List<Position>();

        public MapFactory(IDbManager dbManager, ISystemVariableManager systemVariableManager, ITileLocator tileLocator)
        {            
            this.dbManager = dbManager;
            this.systemVariableManager = systemVariableManager;
            this.tileLocator = tileLocator;
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

                    cityLocations.Add(new Position(x, y));
                }
            }
        }

        public IEnumerable<Position> Locations()
        {
            return cityLocations.AsReadOnly();
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
        
        public bool TooCloseToCities(Position position)
        {
            return Locations().Any(loc => tileLocator.TileDistance(position, 1, loc, 1) <= MIN_DISTANCE_AWAY_FROM_CITIES);
        }
    }
}