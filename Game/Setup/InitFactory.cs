#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Setup {
    enum InitCondition : byte {
        ON_INIT = 1,
        ON_DOWNGRADE = 2
    }

    class InitRecord {
        public Type type;
        public InitCondition condition;
        public string[] parms;
    }

    class InitFactory {
        private static Dictionary<int, List<InitRecord>> dict;

        public static void Init(string filename) {
            if (dict != null)
                return;
            
            dict = new Dictionary<int, List<InitRecord>>();

            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;
                    InitRecord record = new InitRecord();
                    string name = "Game.Logic.Actions." + toks[col["Action"]];
                    record.type = Type.GetType(name, true);
                    if (record.type == null)
                        continue;
                    record.condition = (InitCondition) Enum.Parse(typeof (InitCondition), toks[col["Condition"]], true);
                    record.parms = new string[toks.Length - 4];
                    for (int i = 4; i < toks.Length; ++i) {
                        record.parms[i - 4] = toks[i].Contains("=") ? toks[i].Split('=')[1] : toks[i];
                    }
                    Global.Logger.Info(string.Format("{0}:{1}", int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]]), record.type));

                    int index = int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]]);
                    List<InitRecord> tmp;
                    if (!dict.TryGetValue(index, out tmp)) {
                        tmp = new List<InitRecord>();
                        dict[index] = tmp;
                    }
                    tmp.Add(record);
                }
            }
        }

        public static void InitGameObject(InitCondition condition, GameObject obj, int type, int lvl) {
            if (dict == null)
                return;
            List<InitRecord> tmp;
            if (dict.TryGetValue(type*100 + lvl, out tmp)) {
                foreach (InitRecord each in tmp) {
                    if (each.condition != condition)
                        continue;

                    IScriptable action = (IScriptable) Activator.CreateInstance(each.type, new object[] {});
                    action.ScriptInit(obj, each.parms);
                }
            }
            return;
        }
    }
}