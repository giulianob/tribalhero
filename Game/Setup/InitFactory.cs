#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Setup
{
    enum InitCondition : byte
    {
        OnInit = 1,
        OnDowngrade = 2,
        OnUpgrade = 3,
        OnDestroy = 4,
        OnConvert
    }

    class InitRecord
    {
        public InitCondition Condition { get; set; }
        public string[] Parms { get; set; }
        public Type Type { get; set; }
    }

    class InitFactory
    {
        private static Dictionary<int, List<InitRecord>> dict;

        public static void Init(string filename)
        {
            if (dict != null)
                return;

            dict = new Dictionary<int, List<InitRecord>>();

            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;
                    var record = new InitRecord();
                    string name = "Game.Logic.Actions." + (toks[col["Action"]] + "_passive_action").ToCamelCase();
                    record.Type = Type.GetType(name, true);
                    if (record.Type == null)
                        continue;
                    record.Condition = (InitCondition)Enum.Parse(typeof(InitCondition), toks[col["Condition"]].ToCamelCase(), true);
                    record.Parms = new string[toks.Length - 4];
                    for (int i = 4; i < toks.Length; ++i)
                        record.Parms[i - 4] = toks[i].Contains("=") ? toks[i].Split('=')[1] : toks[i];
                    Global.Logger.Info(string.Format("{0}:{1}", int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]]), record.Type));

                    int index = int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]]);
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

        public static void InitGameObject(InitCondition condition, GameObject obj, int type, int lvl)
        {
            if (dict == null)
                return;
            List<InitRecord> tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp))
            {
                foreach (var each in tmp)
                {
                    if (each.Condition != condition)
                        continue;

                    var action = (IScriptable)Activator.CreateInstance(each.Type, new object[] {});
                    action.ScriptInit(obj, each.Parms);
                }
            }
            return;
        }
    }
}