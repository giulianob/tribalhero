#region

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Game.Data;
using Game.Logic;
using Game.Util;

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

    class InitRecord
    {
        public InitCondition Condition { get; set; }

        public string[] Parms { get; set; }

        public Type Type { get; set; }

        public string NlsDescription { get; set; }
    }

    public class InitFactory
    {
        private readonly Dictionary<int, List<InitRecord>> dict;

        public InitFactory(string filename)
        {
            dict = new Dictionary<int, List<InitRecord>>();

            using (
                    var reader =
                            new CsvReader(
                                    new StreamReader(new FileStream(filename,
                                                                    FileMode.Open,
                                                                    FileAccess.Read,
                                                                    FileShare.ReadWrite))))
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
                    var record = new InitRecord
                    {
                            Type =
                                    Type.GetType(
                                                 "Game.Logic.Actions." +
                                                 (toks[col["Action"]] + "_passive_action").ToCamelCase(),
                                                 true),
                            Condition =
                                    (InitCondition)
                                    Enum.Parse(typeof(InitCondition), toks[col["Condition"]].ToCamelCase(), true),
                            Parms = new string[toks.Length - 4],
                            NlsDescription = toks[col["NlsDesc"]],
                    };

                    for (int i = 4; i <= 8; ++i)
                    {
                        record.Parms[i - 4] = toks[i].Contains("=") ? toks[i].Split('=')[1] : toks[i];
                    }

                    Global.Logger.Info(string.Format("{0}:{1}",
                                                     int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]),
                                                     record.Type));

                    int index = int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]);
                    List<InitRecord> tmp;
                    if (!dict.TryGetValue(index, out tmp))
                    {
                        tmp = new List<InitRecord>();
                        dict[index] = tmp;
                    }
                    tmp.Add(record);
                }
            }
        }

        public void InitGameObject(InitCondition condition, IGameObject obj, int type, int lvl)
        {
            if (dict == null)
            {
                return;
            }
            List<InitRecord> tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp))
            {
                foreach (var each in tmp)
                {
                    if (each.Condition != condition)
                    {
                        continue;
                    }

                    var action = (IScriptable)Activator.CreateInstance(each.Type, new object[] {});

                    // TODO: This needs to be more generic to support more types of actions. Since this is init and all init actions are passive, then this is okay for now.
                    if (action is ScheduledPassiveAction)
                    {
                        ((ScheduledPassiveAction)action).NlsDescription = each.NlsDescription;
                    }

                    action.ScriptInit(obj, each.Parms);
                }
            }
            return;
        }
    }
}