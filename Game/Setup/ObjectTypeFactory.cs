#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Setup
{
    public class ObjectTypeFactory
    {
        private Dictionary<string, List<ushort>> dict;

        public ObjectTypeFactory(string filename)
        {
            dict = new Dictionary<string, List<ushort>>();
            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                List<ushort> set;
                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;
                    if (!dict.TryGetValue(toks[0], out set))
                    {
                        set = new List<ushort>();
                        dict.Add(toks[0], set);
                    }
                    for (int i = 1; i < toks.Length; ++i)
                    {
                        ushort value;
                        if (ushort.TryParse(toks[i], out value))
                        {
                            if (set.Contains(value))
                                throw new Exception("Value already exists");
                            set.Add(value);
                        }
                    }
                }
            }
        }

        public bool IsStructureType(string type, Structure structure)
        {
            if (dict == null)
                return false;
            List<ushort> set;
            if (dict.TryGetValue(type, out set))
                return set.Contains(structure.Type);
            return false;
        }

        public bool IsStructureType(string type, ushort structureType)
        {
            if (dict == null)
                return false;
            List<ushort> set;
            if (dict.TryGetValue(type, out set))
                return set.Contains(structureType);
            return false;
        }

        public ushort[] GetTypes(string type)
        {
            if (dict == null || !dict.ContainsKey(type))
                return new ushort[] {};
            return dict[type].ToArray();
        }

        public bool IsTileType(string type, ushort tileType)
        {
            if (dict == null)
                return false;
            List<ushort> set;
            if (dict.TryGetValue(type, out set))
                return set.Contains(tileType);
            return false;
        }

        public bool HasTileType(string type, IEnumerable<ushort> tileTypes)
        {
            if (dict == null)
                return false;
            List<ushort> set;
            return dict.TryGetValue(type, out set) && tileTypes.Any(tileType => set.Contains(tileType));
        }

        public bool IsAllTileType(string type, IEnumerable<ushort> tileTypes)
        {
            if (dict == null)
                return false;
            List<ushort> set;
            return dict.TryGetValue(type, out set) && tileTypes.All(tileType => set.Contains(tileType));
        }
    }
}