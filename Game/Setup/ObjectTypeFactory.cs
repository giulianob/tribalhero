#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Game.Data;

#endregion

namespace Game.Setup
{
    public class ObjectTypeFactory
    {
        private readonly Dictionary<string, List<ushort>> dict = new Dictionary<string, List<ushort>>();

        [Obsolete("Testing only", true)]
        public ObjectTypeFactory()
        {
        }

        public ObjectTypeFactory(string filename)
        {
            using (
                    var reader =
                            new CsvReader(
                                    new StreamReader(new FileStream(filename,
                                                                    FileMode.Open,
                                                                    FileAccess.Read,
                                                                    FileShare.ReadWrite))))
            {
                String[] toks;
                List<ushort> set;
                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                    {
                        continue;
                    }

                    if (!dict.TryGetValue(toks[0], out set))
                    {
                        set = new List<ushort>();
                        dict.Add(toks[0], set);
                    }

                    for (int i = 1; i < toks.Length; ++i)
                    {
                        ushort value;
                        if (!ushort.TryParse(toks[i], out value))
                        {
                            continue;
                        }

                        if (set.Contains(value))
                        {
                            throw new Exception("Value already exists");
                        }

                        set.Add(value);
                    }
                }
            }
        }

        public virtual bool IsStructureType(string type, IStructure structure)
        {
            List<ushort> set;
            return dict.TryGetValue(type, out set) && set.Contains(structure.Type);
        }

        public virtual bool IsStructureType(string type, ushort structureType)
        {
            List<ushort> set;
            return dict.TryGetValue(type, out set) && set.Contains(structureType);
        }

        public virtual ushort[] GetTypes(string type)
        {
            return !dict.ContainsKey(type) ? new ushort[] {} : dict[type].ToArray();
        }

        public virtual bool IsTileType(string type, ushort tileType)
        {
            List<ushort> set;
            return dict.TryGetValue(type, out set) && set.Contains(tileType);
        }

        public virtual bool HasTileType(string type, IEnumerable<ushort> tileTypes)
        {
            List<ushort> set;
            return dict.TryGetValue(type, out set) && tileTypes.Any(tileType => set.Contains(tileType));
        }

        public virtual bool IsAllTileType(string type, IEnumerable<ushort> tileTypes)
        {
            List<ushort> set;
            return dict.TryGetValue(type, out set) && tileTypes.All(tileType => set.Contains(tileType));
        }
    }
}