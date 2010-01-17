#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Data.Stats;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Setup {
    public enum ClassID : ushort {
        RESOURCE = 50,
        STRUCTURE = 100,
        UNIT = 200,
    }

    public class StructureFactory {
        private static Dictionary<int, StructureBaseStats> dict;

        public static void init(string filename) {
            if (dict != null)
                return;
            dict = new Dictionary<int, StructureBaseStats>();
            init("Game\\Setup\\CSV\\structure.csv");
            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                ) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);
                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;
                    Resource resource = new Resource(int.Parse(toks[col["Crop"]]), int.Parse(toks[col["Gold"]]),
                                                     int.Parse(toks[col["Iron"]]), int.Parse(toks[col["Wood"]]),
                                                     int.Parse(toks[col["Labor"]]));

                    BaseBattleStats stats = new BaseBattleStats(ushort.Parse(toks[col["Type"]]),
                                                                byte.Parse(toks[col["Lvl"]]),
                                                                (WeaponType)
                                                                Enum.Parse(typeof (WeaponType), toks[col["Weapon"]].ToUpper()),
                                                                ArmorType.STONE, ushort.Parse(toks[col["Hp"]]),
                                                                byte.Parse(toks[col["Atk"]]),
                                                                byte.Parse(toks[col["Def"]]),
                                                                byte.Parse(toks[col["Rng"]]),
                                                                byte.Parse(toks[col["Stl"]]),
                                                                byte.Parse(toks[col["Spd"]]), 0,
                                                                Formula.GetRewardPoint(resource,
                                                                                       ushort.Parse(toks[col["Hp"]])));

                    StructureBaseStats basestats = new StructureBaseStats(toks[col["Name"]],
                                                                          ushort.Parse(toks[col["Type"]]),
                                                                          byte.Parse(toks[col["Lvl"]]),
                                                                          byte.Parse(toks[col["Radius"]]),
                                                                          resource, 
                                                                          stats,
                                                                          byte.Parse(toks[col["MaxLabor"]]),
                                                                          int.Parse(toks[col["Time"]]),
                                                                          int.Parse(toks[col["Worker"]]),
                                                                          (ClassID)
                                                                          Enum.Parse(typeof (ClassID),
                                                                                     (toks[col["Class"]]), true));

                    Global.Logger.Info(string.Format("{0}:{1}",
                                                     int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]]),
                                                     toks[col["Name"]]));
                    dict[int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
        }

        public static Resource getCost(int type, int lvl) {
            if (dict == null)
                return null;
            StructureBaseStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return new Resource(tmp.Cost);
            return null;
        }

        internal static int getTime(ushort type, byte lvl) {
            if (dict == null)
                return -1;
            StructureBaseStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.BuildTime;
            return -1;
        }

        public static Structure getStructure(ushort type, byte lvl) {
            return getStructure(null, type, lvl, true);
        }

        public static Structure getStructure(Structure oldStructure, ushort type, byte lvl, bool append) {
            return getStructure(oldStructure, type, lvl, append, true);
        }

        public static Structure getStructure(Structure oldStructure, ushort type, byte lvl, bool append, bool init) {
            if (dict == null)
                return null;
            StructureBaseStats baseStats;

            if (dict.TryGetValue(type*100 + lvl, out baseStats)) {
                //Creating new structure
                if (oldStructure == null) {
                    Structure obj = new Structure(new StructureStats(baseStats));
                    return obj;
                }

                if (append == false) {
                    //Calculate the different in MAXHP between the new and old structures and add it to the current hp if the new one is greater.
                    ushort newHp = oldStructure.Stats.Hp;
                    if (baseStats.Battle.MaxHp > oldStructure.Stats.Base.Battle.MaxHp)
                        newHp =
                            (ushort)
                            (oldStructure.Stats.Hp + (baseStats.Battle.MaxHp - oldStructure.Stats.Base.Battle.MaxHp));

                    oldStructure.Stats = new StructureStats(baseStats);
                    oldStructure.Stats.Hp = newHp;
                    oldStructure.Properties.clear();
                    return null;
                } else if (append) {
                    //Calculate the different in MAXHP between the new and old structures and add it to the current hp if the new one is greater.
                    ushort newHp = oldStructure.Stats.Hp;
                    if (baseStats.Battle.MaxHp > oldStructure.Stats.Base.Battle.MaxHp)
                        newHp =
                            (ushort)
                            (oldStructure.Stats.Hp + (baseStats.Battle.MaxHp - oldStructure.Stats.Base.Battle.MaxHp));

                    oldStructure.Stats = new StructureStats(baseStats);
                    oldStructure.Stats.Hp = newHp;
                    return null;
                }
            }
            return null;
        }

        internal static int getActionWorkerType(Structure structure) {
            if (dict == null)
                return 0;
            StructureBaseStats tmp;
            if (dict.TryGetValue(structure.Type*100 + structure.Lvl, out tmp))
                return tmp.WorkerId;
            return 0;
        }

        internal static string getName(Structure structure) {
            if (dict == null)
                return null;
            StructureBaseStats tmp;
            if (dict.TryGetValue(structure.Type*100 + structure.Lvl, out tmp))
                return tmp.Name;
            return null;
        }
    }
}