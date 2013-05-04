#region

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Game.Data;
using Game.Data.Stats;
using Game.Util;

#endregion

namespace Game.Setup
{
    public class UnitFactory
    {
        private readonly Dictionary<int, IBaseUnitStats> dict = new Dictionary<int, IBaseUnitStats>();

        public void Init(string filename)
        {
            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                {
                    if (reader.Columns[i].Length == 0)
                    {
                        continue;
                    }
                    col.Add(reader.Columns[i], i);
                }
                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                    {
                        continue;
                    }
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
                                                    decimal.Parse(toks[col["Hp"]]),
                                                    decimal.Parse(toks[col["Atk"]]),
                                                    byte.Parse(toks[col["Splash"]]),
                                                    byte.Parse(toks[col["Rng"]]),
                                                    byte.Parse(toks[col["Stl"]]),
                                                    byte.Parse(toks[col["Spd"]]),
                                                    ushort.Parse(toks[col["GrpSize"]]),
                                                    ushort.Parse(toks[col["Carry"]]),
                                                    resource.NormalizedCost);

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

                    dict[int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
        }

        public virtual Resource GetCost(int type, int lvl)
        {
            IBaseUnitStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? new Resource(tmp.Cost) : null;
        }

        public virtual Resource GetUpgradeCost(int type, int lvl)
        {
            IBaseUnitStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? new Resource(tmp.UpgradeCost) : null;
        }

        public virtual IBaseUnitStats GetUnitStats(ushort type, byte lvl)
        {
            IBaseUnitStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp : null;
        }

        public virtual BaseBattleStats GetBattleStats(ushort type, byte lvl)
        {
            IBaseUnitStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp.Battle : null;
        }

        public virtual int GetTime(ushort type, byte lvl)
        {
            IBaseUnitStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp.BuildTime : -1;
        }

        public virtual int GetUpgradeTime(ushort type, byte lvl)
        {
            IBaseUnitStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp.UpgradeTime : -1;
        }

        public virtual string GetName(ushort type, byte lvl)
        {
            IBaseUnitStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp.Name : null;
        }

        public virtual Dictionary<int, IBaseUnitStats> GetList()
        {
            return new Dictionary<int, IBaseUnitStats>(dict);
        }

        public virtual IEnumerable<IBaseUnitStats> AllUnits()
        {
            return dict.Values;
        }

        public virtual void AddType(IBaseUnitStats baseUnitStats)
        {
            dict[baseUnitStats.Type * 100 + baseUnitStats.Lvl] = baseUnitStats;
        }
    }
}