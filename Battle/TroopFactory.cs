using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Game.Util;
using Game.Data;
namespace Game.Fighting {

    public class TroopFactoryRecord {
        public Unit unit;
        public Resource resource;

    }
  /*  public class TroopFactory {

        static Dictionary<int, TroopFactoryRecord> dict;

        static void init() {
            if (dict != null) return;
            dict = new Dictionary<int, TroopFactoryRecord>();
            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream("c:\\source\\GameServer\\Game\\Setup\\CSV\\unit.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i) {
                    col.Add(reader.Columns[i], i);
                }

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0) continue;

                    if (ushort.Parse(toks[col["Type"]]) < 2000) continue;
                    Unit unit = new Unit();

                    unit.type = ushort.Parse(toks[col["Type"]]);
                    unit.name = toks[col["Name"]];
                    unit.lvl = byte.Parse(toks[col["Lvl"]]);
                    unit.stats.atk = byte.Parse(toks[col["Atk"]]);
                    unit.stats.def = byte.Parse(toks[col["Def"]]);
                    unit.stats.range = byte.Parse(toks[col["Rng"]]);
                    unit.stats.vision = byte.Parse(toks[col["Vsn"]]);
                    unit.stats.stealth = byte.Parse(toks[col["Stl"]]);
                    unit.stats.weapon_type = 0;
                    unit.stats.armor_type = 0;

                    Resource resource = new Resource(int.Parse(toks[col["Crop"]]),
                                            int.Parse(toks[col["Gold"]]),
                                            int.Parse(toks[col["Iron"]]),
                                            int.Parse(toks[col["Wood"]]));

                    TroopFactoryRecord record = new TroopFactoryRecord();
                    record.unit = unit;
                    record.resource = resource;
                    //                    Console.Out.WriteLine("{0}:{1}", int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]), toks[col["Name"]]);
                    dict[int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]])] = record;
                }

            }
        }

        static public Unit getUnit(int type, int lvl) {
            if (dict == null) init();
            TroopFactoryRecord tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) {
                return (Unit)tmp.unit.Clone();
            }
            return null;
        }
        static public Resource getResource(int type, int lvl) {
            if (dict == null) init();
            TroopFactoryRecord tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) {
                return tmp.resource;
            }
            return null;
        }

        public static List<TroopFactoryRecord> getFullList() {
            if (dict == null) init();
            return new List<TroopFactoryRecord>(dict.Values);
        }
    }*/
}
