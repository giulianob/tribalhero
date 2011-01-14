#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Setup {
    public class ObjectTypeFactory {
        private static Dictionary<string, List<ushort>> dict;

        public static void init(string filename) {
            if (dict != null)
                return;
            dict = new Dictionary<string, List<ushort>>();
            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                ) {
                String[] toks;
                List<ushort> set;
                Dictionary<string, int> col = new Dictionary<string, int>();
                ushort value;
                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;
                    if (!dict.TryGetValue(toks[0], out set)) {
                        set = new List<ushort>();
                        dict.Add(toks[0], set);
                    }
                    for (int i = 1; i < toks.Length; ++i) {
                        if (ushort.TryParse(toks[i], out value)) {
                            if (set.Contains(value))
                                throw new Exception("Value already exists");
                            set.Add(value);
                        }
                    }
                }
            }
        }

        public static bool IsStructureType(string type, Structure structure) {
            if (dict == null)
                return false;
            List<ushort> set;
            if (dict.TryGetValue(type, out set))
                return set.Contains(structure.Type);
            return false;
        }

        public static bool IsStructureType(string type, ushort structureType)
        {
            if (dict == null)
                return false;
            List<ushort> set;
            if (dict.TryGetValue(type, out set))
                return set.Contains(structureType);
            return false;
        }

        public static ushort[] GetTypes(string type) {
            if (dict == null || !dict.ContainsKey(type)) return new ushort[] { };
            return dict[type].ToArray();
        }

        public static bool IsTileType(string type, ushort tileType) {
            if (dict == null)
                return false;
            List<ushort> set;
            if (dict.TryGetValue(type, out set))
                return set.Contains(tileType);
            return false;
        }

        public static bool HasTileType(string type, IEnumerable<ushort> tileTypes) {
            if (dict == null)
                return false;
            List<ushort> set;
            return dict.TryGetValue(type, out set) && tileTypes.Any(tileType => set.Contains(tileType));
        }

        public static bool IsAllTileType(string type, IEnumerable<ushort> tileTypes) {
            if (dict == null)
                return false;
            List<ushort> set;
            return dict.TryGetValue(type, out set) && tileTypes.All(tileType => set.Contains(tileType));
        }
    }
}