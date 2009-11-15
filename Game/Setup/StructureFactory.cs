using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Util;
using System.IO;
using System.Collections.Specialized;
using Game.Logic;

namespace Game.Setup {
    enum ClassID : ushort {
        RESOURCE = 50,
        STRUCTURE = 100,
        UNIT = 200,
    }

    class StructureBaseStats {
        public string name;
        public byte lvl;
        public Resource resource;
        public StructureStats stats;
        public int build_time;
        public ClassID base_class;
        public int worker_id;

        public StructureBaseStats(string name, byte lvl, Resource resource, BattleStats stats, byte maxLabor, int build_time, ClassID base_class) {
            this.name = name;
            this.lvl = lvl;
            this.resource = resource;
            this.stats = new StructureStats(stats, maxLabor);
            this.build_time = build_time;
            this.base_class = base_class;
        }
    }

    public class StructureFactory {
        static Dictionary<int, StructureBaseStats> dict;

        public static void init(string filename) {
            if (dict != null) return;
            dict = new Dictionary<int, StructureBaseStats>();
            init("Game\\Setup\\CSV\\structure.csv"); using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i) {
                    col.Add(reader.Columns[i], i);
                }
                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0) continue;
                    Resource resource = new Resource(int.Parse(toks[col["Crop"]]),
                                            int.Parse(toks[col["Gold"]]),
                                            int.Parse(toks[col["Iron"]]),
                                            int.Parse(toks[col["Wood"]]),
                                            int.Parse(toks[col["Labor"]]));

                    BattleStats stats = new BattleStats((WeaponType)Enum.Parse(typeof(WeaponType), toks[col["Weapon"]]),
                                            ArmorType.Stone, 
                                            ushort.Parse(toks[col["Hp"]]),
                                            byte.Parse(toks[col["Atk"]]),
                                            byte.Parse(toks[col["Def"]]),
                                            byte.Parse(toks[col["Rng"]]),
                                            byte.Parse(toks[col["Stl"]]),
                                            byte.Parse(toks[col["Spd"]]),
                                            0,
                                            Formula.GetRewardPoint(resource,ushort.Parse(toks[col["Hp"]])));


                    StructureBaseStats basestats = new StructureBaseStats(toks[col["Name"]],
                                                    byte.Parse(toks[col["Lvl"]]),
                                                    resource,
                                                    stats,
                                                    byte.Parse(toks[col["MaxLabor"]]),
                                                    int.Parse(toks[col["Time"]]),
                                                    (ClassID)Enum.Parse(typeof(ClassID), (toks[col["Class"]]), true));
                    basestats.worker_id = int.Parse(toks[col["Worker"]]);
                    Global.Logger.Info(string.Format("{0}:{1}", int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]), toks[col["Name"]]));
                    dict[int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
        }

        static public Resource getCost(int type, int lvl) {
            if (dict == null) return null;
            StructureBaseStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (Resource)tmp.resource.Clone();
            return null;
        }

        internal static StructureStats getStats(ushort type, byte lvl) {
            if (dict == null) return null;
            StructureBaseStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (StructureStats)tmp.stats;
            return null;
        }
        internal static int getTime(ushort type, byte lvl) {
            if (dict == null) return -1;
            StructureBaseStats tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (int)tmp.build_time;
            return -1;
        }

        public static Structure getStructure(ushort type, byte lvl) {
            return getStructure(null, type, lvl, true);
        }

        public static Structure getStructure(Structure oldStructure, ushort type, byte lvl, bool append) {
            return getStructure(oldStructure, type, lvl, append, true);
        }

        public static Structure getStructure(Structure oldStructure, ushort type, byte lvl, bool append, bool init) {
            if (dict == null) return null;
            StructureBaseStats tmp;

            if (dict.TryGetValue(type * 100 + lvl, out tmp)) {
                if (oldStructure == null) {
                    Structure obj = new Structure(type, lvl, tmp.stats);
                    return obj;
                }
                else if (append == false) {
                    Global.dbManager.Delete(oldStructure.Properties);
                    oldStructure.Type = type;
                    oldStructure.Lvl = lvl;                    
                    if( tmp.stats.Battle.MaxHp> oldStructure.Stats.Battle.MaxHp ) {
                        oldStructure.Hp += (ushort)(tmp.stats.Battle.MaxHp - oldStructure.Stats.Battle.MaxHp);                                     
                    }
                    oldStructure.Stats = tmp.stats;                     
                    oldStructure.Properties = new StructureProperties(oldStructure);
                    return null;
                }
                else if (append == true) {
                    oldStructure.Type = type;
                    oldStructure.Lvl = lvl;
                    if (tmp.stats.Battle.MaxHp > oldStructure.Stats.Battle.MaxHp) {
                        oldStructure.Hp += (ushort)(tmp.stats.Battle.MaxHp - oldStructure.Stats.Battle.MaxHp);
                    }
                    oldStructure.Stats = tmp.stats;
                    return null;
                }
            }
            return null;
        }

        internal static void getProperties() {
        }



        internal static int getActionWorkerType(Structure structure) {
            if (dict == null) return 0;
            StructureBaseStats tmp;
            if (dict.TryGetValue(structure.Type * 100 + structure.Lvl, out tmp)) {
                return tmp.worker_id;
            }
            return 0;
        }

        internal static string getName(Structure structure) {
            if (dict == null) return null;
            StructureBaseStats tmp;
            if (dict.TryGetValue(structure.Type * 100 + structure.Lvl, out tmp)) {
                return tmp.name;
            }
            return null;
        }
    }
}
