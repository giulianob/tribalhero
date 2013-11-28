#region

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Game.Data;
using Game.Logic.Conditons;
using Game.Logic.Triggers;
using Game.Util;
using Ninject;

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
        private readonly ICityTriggerManager cityTriggerManager;

        private readonly IDynamicActionFactory dynamicActionFactory;

        private readonly IKernel kernel;

        private readonly ICityEventFactory cityEventFactory;

        public InitFactory(ICityTriggerManager cityTriggerManager, IDynamicActionFactory dynamicActionFactory, IKernel kernel, ICityEventFactory cityEventFactory)
        {
            this.cityTriggerManager = cityTriggerManager;
            this.dynamicActionFactory = dynamicActionFactory;
            this.kernel = kernel;
            this.cityEventFactory = cityEventFactory;
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
                    
                    var conditionType = Type.GetType("Game.Logic.Triggers.Conditions." + (toks[col["Condition"]] + "_condition").ToCamelCase(), true);
                    var condition = (IDynamicCondition)kernel.Get(conditionType);

                    string[] conditionParms = new string[5];
                    for (int i = 0; i <= 4; ++i)
                    {
                        conditionParms[i] = toks[i + 1].Contains("=") ? toks[i + 1].Split('=')[1] : toks[i + 1];
                    }
                    condition.SetParameters(conditionParms);

                    var actionType = Type.GetType("Game.Logic.Actions." + (toks[col["Action"]] + "_passive_action").ToCamelCase(), true);
                    var actionNlsDescription = toks[col["NlsDesc"]];

                    var action = dynamicActionFactory.CreateDynamicAction(actionType, actionNlsDescription);
                             
                    for (int i = 0; i <= 4; ++i)
                    {
                        action.Parms[i] = toks[i + 7].Contains("=") ? toks[i + 7].Split('=')[1] : toks[i + 7];
                    }

                    cityTriggerManager.AddTrigger(condition, action);
                }
            }
        }
        
        public void InitGameObject(InitCondition condition, IStructure structure, ushort type, byte lvl)
        {
            switch (condition)
            {
                case InitCondition.OnInit:
                    cityTriggerManager.Process(cityEventFactory.CreateStructureInitEvent(structure, type, lvl));
                    break;
                case InitCondition.OnUpgrade:
                    cityTriggerManager.Process(cityEventFactory.CreateStructureUpgradeEvent(structure, type, lvl));
                    break;
                case InitCondition.OnDowngrade:
                    cityTriggerManager.Process(cityEventFactory.CreateStructureDowngradeEvent(structure, type, lvl));
                    break;
                case InitCondition.OnConvert:
                    cityTriggerManager.Process(cityEventFactory.CreateStructureConvertEvent(structure, type, lvl));
                    break;
            }
        }
    }
}