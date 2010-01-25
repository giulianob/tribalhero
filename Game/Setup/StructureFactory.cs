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
    public enum ClassId : ushort {
        RESOURCE = 50,
        STRUCTURE = 100,
        UNIT = 200,
    }

    public class StructureFactory {
        private static Dictionary<int, StructureBaseStats> dict;

        public static void Init(string filename) {
            if (dict != null)
                return;
            dict = new Dictionary<int, StructureBaseStats>();
            Init("Game\\Setup\\CSV\\structure.csv");
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
                                                                          (ClassId)
                                                                          Enum.Parse(typeof (ClassId),
                                                                                     (toks[col["Class"]]), true));

                    Global.Logger.Info(string.Format("{0}:{1}",
                                                     int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]]),
                                                     toks[col["Name"]]));
                    dict[int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
        }

        public static Resource GetCost(int type, int lvl) {
            if (dict == null)
                return null;
            StructureBaseStats tmp;
            return dict.TryGetValue(type*100 + lvl, out tmp) ? new Resource(tmp.Cost) : null;
        }

        internal static int GetTime(ushort type, byte lvl) {
            if (dict == null)
                return -1;
            StructureBaseStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.BuildTime;
            return -1;
        }

        public static Structure GetStructure(ushort type, byte lvl) {
            return GetStructure(null, type, lvl, true);
        }

        public static Structure GetStructure(Structure oldStructure, ushort type, byte lvl, bool append) {
            return GetStructure(oldStructure, type, lvl, append, true);
        }

        public static Structure GetStructure(Structure oldStructure, ushort type, byte lvl, bool append, bool init) {
            if (dict == null)
                return null;
            StructureBaseStats baseStats;

            if (dict.TryGetValue(type*100 + lvl, out baseStats)) {
                //Creating new structure
                if (oldStructure == null) {
                    Structure obj = new Structure(new StructureStats(baseStats));
                    return obj;
                }

                if (!append) {
                    //Calculate the different in MAXHP between the new and old structures and Add it to the current hp if the new one is greater.
                    ushort newHp = oldStructure.Stats.Hp;
                    if (baseStats.Battle.MaxHp > oldStructure.Stats.Base.Battle.MaxHp)
                        newHp =
                            (ushort)
                            (oldStructure.Stats.Hp + (baseStats.Battle.MaxHp - oldStructure.Stats.Base.Battle.MaxHp));

                    oldStructure.Stats = new StructureStats(baseStats) {Hp = newHp};
                    oldStructure.Properties.clear();                    
                }
                else {
                    //Calculate the different in MAXHP between the new and old structures and Add it to the current hp if the new one is greater.
                    StructureStats oldStats = oldStructure.Stats;

                    ushort newHp = oldStats.Hp;
                    if (baseStats.Battle.MaxHp > oldStats.Base.Battle.MaxHp)
                        newHp = (ushort)(oldStats.Hp + (baseStats.Battle.MaxHp - oldStats.Base.Battle.MaxHp));

                    oldStructure.Stats = new StructureStats(baseStats) {Hp = newHp, Labor = oldStats.Labor};
                }
            }

            return null;
        }

        internal static int GetActionWorkerType(Structure structure) {
            if (dict == null)
                return 0;
            StructureBaseStats tmp;
            return dict.TryGetValue(structure.Type*100 + structure.Lvl, out tmp) ? tmp.WorkerId : 0;
        }

        internal static string GetName(Structure structure) {
            if (dict == null)
                return null;
            StructureBaseStats tmp;
            return dict.TryGetValue(structure.Type*100 + structure.Lvl, out tmp) ? tmp.Name : null;
        }
    }
}