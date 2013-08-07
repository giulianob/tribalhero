#region

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Game.Data;
using Game.Data.Stats;
using Game.Util;
using Ninject;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Setup
{
    public class StructureCsvFactory : IStructureCsvFactory
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly Dictionary<int, StructureBaseStats> dict = new Dictionary<int, StructureBaseStats>();
        
        public void Init(string filename)
        {
            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                {
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
                                                    0,
                                                    0,
                                                    0);
                    int workerId = int.Parse(toks[col["Worker"]]);

                    if (workerId == 0)
                    {
                        workerId = byte.Parse(toks[col["Lvl"]]) == 0
                                           ? 0
                                           : Ioc.Kernel.Get<ActionRequirementFactory>()
                                                .GetActionRequirementRecordBestFit(int.Parse(toks[col["Type"]]), byte.Parse(toks[col["Lvl"]]))
                                                .Id;
                    }

                    var basestats = new StructureBaseStats(toks[col["Name"]],
                                                           toks[col["SpriteClass"]],
                                                           ushort.Parse(toks[col["Type"]]),
                                                           byte.Parse(toks[col["Lvl"]]),
                                                           byte.Parse(toks[col["Radius"]]),
                                                           resource,
                                                           stats,
                                                           ushort.Parse(toks[col["MaxLabor"]]),
                                                           int.Parse(toks[col["Time"]]),
                                                           workerId,
                                                           byte.Parse(toks[col["Size"]]));

                    logger.Info(string.Format("{0}:{1}", int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]), toks[col["Name"]]));

                    dict[int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]])] = basestats;
                }
            }
        }

        public Resource GetCost(ushort type, int lvl)
        {
            StructureBaseStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? new Resource(tmp.Cost) : null;
        }

        public int GetTime(ushort type, byte lvl)
        {
            StructureBaseStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp.BuildTime : -1;
        }

        public void GetUpgradedStructure(IStructure structure, ushort type, byte lvl)
        {
            StructureBaseStats baseStats;
            if (!dict.TryGetValue(type * 100 + lvl, out baseStats))
            {
                throw new Exception(String.Format("Structure not found in csv type[{0}] lvl[{1}]!", type, lvl));
            }

            //Calculate the different in MAXHP between the new and old structures and Add it to the current hp if the new one is greater.
            var oldStats = structure.Stats;

            decimal newHp = oldStats.Hp;
            if (baseStats.Battle.MaxHp > oldStats.Base.Battle.MaxHp)
            {
                newHp = oldStats.Hp + (baseStats.Battle.MaxHp - oldStats.Base.Battle.MaxHp);
            }
            else if (newHp > baseStats.Battle.MaxHp)
            {
                newHp = baseStats.Battle.MaxHp;
            }

            ushort newLabor = oldStats.Labor;
            if (baseStats.MaxLabor > 0 && newLabor > baseStats.MaxLabor)
            {
                newLabor = baseStats.MaxLabor;
            }

            structure.Stats = new StructureStats(baseStats) {Hp = newHp, Labor = newLabor};
        }

        public int GetActionWorkerType(IStructure structure)
        {
            StructureBaseStats tmp;
            return dict.TryGetValue(structure.Type * 100 + structure.Lvl, out tmp) ? tmp.WorkerId : 0;
        }

        public string GetName(ushort type, byte lvl)
        {
            StructureBaseStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp.Name : null;
        }

        public virtual IStructureBaseStats GetBaseStats(ushort type, byte lvl)
        {
            StructureBaseStats tmp;
            return dict.TryGetValue(type * 100 + lvl, out tmp) ? tmp : null;
        }

        public IEnumerable<IStructureBaseStats> AllStructures()
        {
            return dict.Values;
        }
    }
}