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
    public class UnitFactory {
        public static Dictionary<int, BaseUnitStats> dict;

        public static void init(string filename) {
            if (dict != null)
                return;
            dict = new Dictionary<int, BaseUnitStats>();
            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                ) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i) {
                    if (reader.Columns[i].Length == 0)
                        continue;
                    col.Add(reader.Columns[i], i);
                }
                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;
                    Resource resource = new Resource(int.Parse(toks[col["Crop"]]), int.Parse(toks[col["Gold"]]),
                                                     int.Parse(toks[col["Iron"]]), int.Parse(toks[col["Wood"]]),
                                                     int.Parse(toks[col["Labor"]]));

                    Resource upgrade_resource = new Resource(int.Parse(toks[col["UpgrdCrop"]]),
                                                             int.Parse(toks[col["UpgrdGold"]]),
                                                             int.Parse(toks[col["UpgrdIron"]]),
                                                             int.Parse(toks[col["UpgrdWood"]]),
                                                             int.Parse(toks[col["UpgrdLabor"]]));

                    BaseBattleStats stats = new BaseBattleStats(ushort.Parse(toks[col["Type"]]),
                                                                byte.Parse(toks[col["Lvl"]]),
                                                                (WeaponType)
                                                                Enum.Parse(typeof(WeaponType), toks[col["Weapon"]].ToUpper()),
                                                                (ArmorType)
                                                                Enum.Parse(typeof(ArmorType), toks[col["Armor"]].ToUpper()),
                                                                ushort.Parse(toks[col["Hp"]]),
                                                                byte.Parse(toks[col["Atk"]]),
                                                                byte.Parse(toks[col["Def"]]),
                                                                byte.Parse(toks[col["Rng"]]),
                                                                byte.Parse(toks[col["Stl"]]),
                                                                byte.Parse(toks[col["Spd"]]),
                                                                ushort.Parse(toks[col["GrpSize"]]),
                                                                Formula.GetRewardPoint(resource,
                                                                                       ushort.Parse(toks[col["Hp"]])));

                    BaseUnitStats basestats = new BaseUnitStats(toks[col["Name"]], ushort.Parse(toks[col["Type"]]),
                                                                byte.Parse(toks[col["Lvl"]]), resource, upgrade_resource,
                                                                stats, int.Parse(toks[col["Time"]]),
                                                                int.Parse(toks[col["UpgrdTime"]]),
                                                                byte.Parse(toks[col["Upkeep"]]));

                    dict[int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
        }

        public static Resource getCost(int type, int lvl) {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return new Resource(tmp.Cost);
            return null;
        }

        public static Resource getUpgradeCost(int type, int lvl) {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return new Resource(tmp.UpgradeCost);
            return null;
        }

        public static BaseUnitStats getUnitStats(ushort type, byte lvl) {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp;
            return null;
        }

        internal static BaseBattleStats getBattleStats(ushort type, byte lvl) {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.Battle;
            return null;
        }

        internal static int getTime(ushort type, byte lvl) {
            if (dict == null)
                return -1;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.BuildTime;
            return -1;
        }

        internal static int getUpgradeTime(ushort type, byte lvl) {
            if (dict == null)
                return -1;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.UpgradeTime;
            return -1;
        }

        public static string getName(ushort type, byte lvl) {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.Name;
            return null;
        }

        public static Dictionary<int, BaseUnitStats> getList() {
            return new Dictionary<int, BaseUnitStats>(dict);
        }
    }
}