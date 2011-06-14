#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Battle;
using Game.Data;
using Game.Data.Stats;
using Game.Util;
using System.Linq;
#endregion

namespace Game.Setup
{
    public class UnitFactory
    {
        private static Dictionary<int, BaseUnitStats> dict;

        public static void Init(string filename)
        {
            if (dict != null)
                return;
            dict = new Dictionary<int, BaseUnitStats>();
            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                {
                    if (reader.Columns[i].Length == 0)
                        continue;
                    col.Add(reader.Columns[i], i);
                }
                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;
                    var resource = new Resource(int.Parse(toks[col["Crop"]]),
                                                int.Parse(toks[col["Gold"]]),
                                                int.Parse(toks[col["Iron"]]),
                                                int.Parse(toks[col["Wood"]]),
                                                int.Parse(toks[col["Labor"]]));

                    var upgradeResource = new Resource(int.Parse(toks[col["UpgrdCrop"]]),
                                                       int.Parse(toks[col["UpgrdGold"]]),
                                                       int.Parse(toks[col["UpgrdIron"]]),
                                                       int.Parse(toks[col["UpgrdWood"]]),
                                                       int.Parse(toks[col["UpgrdLabor"]]));

                    var stats = new BaseBattleStats(ushort.Parse(toks[col["Type"]]),
                                                    byte.Parse(toks[col["Lvl"]]),
                                                    (WeaponType)Enum.Parse(typeof(WeaponType), toks[col["Weapon"]].ToCamelCase()),
                                                    (WeaponClass)Enum.Parse(typeof(WeaponClass), toks[col["WpnClass"]].ToCamelCase()),
                                                    (ArmorType)Enum.Parse(typeof(ArmorType), toks[col["Armor"]].ToCamelCase()),
                                                    (ArmorClass)Enum.Parse(typeof(ArmorClass), toks[col["ArmrClass"]].ToCamelCase()),
                                                    ushort.Parse(toks[col["Hp"]]),
                                                    ushort.Parse(toks[col["Atk"]]),
                                                    byte.Parse(toks[col["Splash"]]),
                                                    ushort.Parse(toks[col["Def"]]),
                                                    byte.Parse(toks[col["Rng"]]),
                                                    byte.Parse(toks[col["Stl"]]),
                                                    byte.Parse(toks[col["Spd"]]),
                                                    ushort.Parse(toks[col["GrpSize"]]),
                                                    ushort.Parse(toks[col["Carry"]]));

                    var basestats = new BaseUnitStats(toks[col["Name"]],
                                                      toks[col["SpriteClass"]],
                                                      ushort.Parse(toks[col["Type"]]),
                                                      byte.Parse(toks[col["Lvl"]]),
                                                      resource,
                                                      upgradeResource,
                                                      stats,
                                                      int.Parse(toks[col["Time"]]),
                                                      int.Parse(toks[col["UpgrdTime"]]),
                                                      byte.Parse(toks[col["Upkeep"]]));

                    dict[int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
            using (var fs = new StreamWriter(File.Open("ratio.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))) {
                fs.Write("Header,");
                foreach (var kvp in UnitFactory.GetList().Where(x => (x.Value.Lvl % 100) == 1)) {
                    fs.Write(kvp.Value.Name + ",");
                }
                fs.WriteLine("Min,Max,Avg");

                foreach (var kvp in UnitFactory.GetList().Where(x => (x.Value.Lvl % 100) == 1)) {
                    double sum = 0;
                    int count = 0;
                    double min = double.MaxValue;
                    double max = double.MinValue;
                    fs.Write(kvp.Value.Name + ",");
                    foreach (var kvp2 in UnitFactory.GetList().Where(x => (x.Value.Lvl % 100) == 1)) {
                        double classModifer = BattleFormulas.GetArmorClassModifier(kvp.Value.Battle.WeaponClass, kvp2.Value.Battle.ArmorClass);
                        double typeModifer = BattleFormulas.GetArmorTypeModifier(kvp.Value.Battle.Weapon, kvp2.Value.Battle.Armor);
                        double modifier = classModifer * typeModifer;
                       // fs.Write(String.Format("{0}x{1}={2},", classModifer, typeModifer, modifier));
                        fs.Write(String.Format("{0},", modifier));
                        min = Math.Min(modifier, min);
                        max = Math.Max(modifier, max);
                        sum += modifier;
                        ++count;
                    }
                    fs.WriteLine(string.Format("{0},{1},{2}", min, max, sum / count));
                }

            }
        }

        public static Resource GetCost(int type, int lvl)
        {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return new Resource(tmp.Cost);
            return null;
        }

        public static Resource GetUpgradeCost(int type, int lvl)
        {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return new Resource(tmp.UpgradeCost);
            return null;
        }

        public static BaseUnitStats GetUnitStats(ushort type, byte lvl)
        {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp;
            return null;
        }

        internal static BaseBattleStats GetBattleStats(ushort type, byte lvl)
        {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.Battle;
            return null;
        }

        internal static int GetTime(ushort type, byte lvl)
        {
            if (dict == null)
                return -1;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.BuildTime;
            return -1;
        }

        internal static int GetUpgradeTime(ushort type, byte lvl)
        {
            if (dict == null)
                return -1;
            BaseUnitStats tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp.UpgradeTime;
            return -1;
        }

        public static string GetName(ushort type, byte lvl)
        {
            if (dict == null)
                return null;
            BaseUnitStats tmp;
            return dict.TryGetValue(type*100 + lvl, out tmp) ? tmp.Name : null;
        }

        public static Dictionary<int, BaseUnitStats> GetList()
        {
            return new Dictionary<int, BaseUnitStats>(dict);
        }

        public static IEnumerable<BaseUnitStats> AllUnits()
        {
            return dict.Values;
        }
    }
}