#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Game.Data;
using Game.Logic;
using Game.Logic.Conditons;
using Game.Logic.Triggers;
using Game.Logic.Triggers.Events;
using Game.Util;
using Ninject;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Setup
{
    public enum InitCondition : byte
    {
        OnInit = 1,

        OnDowngrade = 2,

        OnUpgrade = 3,

        OnDestroy = 4,

        OnConvert = 5,
    }

    public class InitFactory
    {
        private readonly CityTriggerManager cityTriggerManager;

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public InitFactory(CityTriggerManager cityTriggerManager)
        {
            this.cityTriggerManager = cityTriggerManager;
        }

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

                    var condition =
                            (IDynamicCondition)
                            Activator.CreateInstance(Type.GetType("Game.Logic.Triggers.Conditions." + (toks[col["Condition"]] + "_condition").ToCamelCase(), true),
                                                     new object[] {});

                    string[] conditionParms = new string[5];
                    for (int i = 0; i <= 4; ++i)
                    {
                        conditionParms[i] = toks[i + 1].Contains("=") ? toks[i + 1].Split('=')[1] : toks[i + 1];
                    }
                    condition.SetParameters(conditionParms);

                    var action = new DynamicAction
                    {
                            Type = Type.GetType("Game.Logic.Actions." + (toks[col["Action"]] + "_passive_action").ToCamelCase(), true),
                            Parms = new string[5],
                            NlsDescription = toks[col["NlsDesc"]],
                    };

                    for (int i = 0; i <= 4; ++i)
                    {
                        action.Parms[i] = toks[i + 7].Contains("=") ? toks[i + 7].Split('=')[1] : toks[i + 7];
                    }

                   // logger.Info(string.Format("{0}:{1}", int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]), record.Type));
                    Ioc.Kernel.Get<CityTriggerManager>().AddTrigger(condition, action);
                }
            }
        }
        
        public void InitGameObject(InitCondition condition, IStructure structure, ushort type, byte lvl)
        {
            switch (condition)
            {
                case InitCondition.OnInit:
                    cityTriggerManager.Process(new StructureInitEvent(structure, type, lvl));
                    break;
                case InitCondition.OnUpgrade:
                    cityTriggerManager.Process(new StructureUpgradeEvent(structure, type, lvl));
                    break;
                case InitCondition.OnDowngrade:
                    cityTriggerManager.Process(new StructureDowngradeEvent(structure, type, lvl));
                    break;
                case InitCondition.OnConvert:
                    cityTriggerManager.Process(new StructureConvertEvent(structure, type, lvl));
                    break;
            }
        }
    }
}