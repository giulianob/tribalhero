using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Util;
using System.IO;
using Game.Logic;
using Game.Logic.Actions;

namespace Game.Setup {

    public class ObjectTypeFactory {
        static Dictionary<string, HashSet<ushort>> dict;

        public static void init(string filename) {
            if (dict != null) return;
            dict = new Dictionary<string, HashSet<ushort>>();
            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks=reader.Columns;
                HashSet<ushort> set;
                Dictionary<string, int> col = new Dictionary<string, int>();
                ushort value;
                do {
                    if (toks[0].Length <= 0) continue;
                    if (!dict.TryGetValue(toks[0], out set)) {
                        set = new HashSet<ushort>();
                        dict.Add(toks[0], set);
                    }
                    for (int i = 1; i < toks.Length; ++i) {
                        if (ushort.TryParse(toks[i],out value)) {
                            set.Add(value);
                        }
                    }
                } while ((toks = reader.ReadRow()) != null);
            }
        }

        static public bool IsStructureType(string type, Structure structure) {
            if (dict == null) return false;
            HashSet<ushort> set;
            if (dict.TryGetValue(type, out set)) {
                return set.Contains(structure.Type);
            }
            return false;
        }
        static public bool IsTileType(string type, ushort tileType) {
            if (dict == null) return false;
            HashSet<ushort> set;
            if (dict.TryGetValue(type, out set)) {
                return set.Contains(tileType);
            }
            return false;
        }
    }
}
