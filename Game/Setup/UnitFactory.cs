using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Util;
using System.IO;
using System.Collections.Specialized;
using Game.Logic;

namespace Game.Setup {
    public class UnitStats {
        public string name;
        public Resource resource;
        public Resource upgrade_resource;
        public BattleStats stats;
        public int type;
        public byte lvl;
        public int build_time;
        public int upgrade_time;
        public byte upkeep;

        public UnitStats(string name,int type, byte lvl, Resource resource, Resource upgrade_resource, BattleStats stats, int build_time, int upgrade_time, byte upkeep) {
            this.name = name;
            this.type = type;
            this.lvl = lvl;
            this.resource = resource;
            this.upgrade_resource = upgrade_resource;
            this.stats = stats;
            this.build_time = build_time;
            this.upgrade_time = upgrade_time;
            this.upkeep = upkeep;
        }
    }

    public class UnitFactory {
        public static Dictionary<int, UnitStats> dict;
        
        public static void init(string filename) {
            if (dict != null) return;
            dict = new Dictionary<int, UnitStats>();
            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i) {
                    if (reader.Columns[i].Length == 0) continue;
                    col.Add(reader.Columns[i], i);
                }
                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0) continue;
                    Resource resource = new Resource(int.Parse(toks[col["Crop"]]),
                                            int.Parse(toks[col["Gold"]]),
                                            int.Parse(toks[col["Iron"]]),
                                            int.Parse(toks[col["Wood"]]),
                                            int.Parse(toks[col["Labor"]]));
                    Resource upgrade_resource = new Resource(int.Parse(toks[col["UpgrdCrop"]]),
                                            int.Parse(toks[col["UpgrdGold"]]),
                                            int.Parse(toks[col["UpgrdIron"]]),
                                            int.Parse(toks[col["UpgrdWood"]]),
                                            int.Parse(toks[col["UpgrdLabor"]]));
                    
                    BattleStats stats = new BattleStats((WeaponType)Enum.Parse(typeof(WeaponType),toks[col["Weapon"]]),
                                            (ArmorType)Enum.Parse(typeof(ArmorType), toks[col["Armor"]]),
                                            ushort.Parse(toks[col["Hp"]]),
                                            byte.Parse(toks[col["Atk"]]),
                                            byte.Parse(toks[col["Def"]]),
                                            byte.Parse(toks[col["Rng"]]),
                                            byte.Parse(toks[col["Stl"]]),
                                            byte.Parse(toks[col["Spd"]]),
                                            ushort.Parse(toks[col["GrpSize"]]),
                                            Formula.GetRewardPoint(resource, ushort.Parse(toks[col["Hp"]])));


                    UnitStats basestats = new UnitStats(toks[col["Name"]],
                                                    int.Parse(toks[col["Type"]]),
                                                    byte.Parse(toks[col["Lvl"]]),
                                                    resource,
                                                    upgrade_resource,
                                                    stats,
                                                    int.Parse(toks[col["Time"]]),
                                                    int.Parse(toks[col["UpgrdTime"]]),
                                                    byte.Parse(toks[col["Upkeep"]]));

                    dict[int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
        }

        static public Resource getCost(int type, int lvl) {
            if (dict == null) return null;
            UnitStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (Resource)tmp.resource.Clone();
            return null;
        }
        static public Resource getUpgradeCost(int type, int lvl) {
            if (dict == null) return null;
            UnitStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (Resource)tmp.upgrade_resource.Clone();
            return null;
        }

        public static UnitStats getUnitStats(ushort type, byte lvl) {
            if (dict == null) return null;
            UnitStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return tmp;
            return null;
        }

        internal static BattleStats getStats(ushort type, byte lvl) {
            if (dict == null) return null;
            UnitStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (BattleStats)tmp.stats.Clone();
            return null;
        }
        internal static int getTime(ushort type, byte lvl) {
            if (dict == null) return -1;
            UnitStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (int)tmp.build_time;
            return -1;
        }
        internal static int getUpgradeTime(ushort type, byte lvl) {
            if (dict == null) return -1;
            UnitStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (int)tmp.upgrade_time;
            return -1;
        }
        public static string getName(ushort type, byte lvl) {
            if (dict == null) return null;
            UnitStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return tmp.name;
            return null;
        }
        public static Dictionary<int, UnitStats> getList() {
            return new Dictionary<int, UnitStats>(dict);
        }

    }
}
