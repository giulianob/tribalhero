#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Setup {
    public class ActionRecord {
        public byte max;
        public List<ActionRequirement> list;
    }

    public class ActionFactory {
        private static Dictionary<int, ActionRecord> dict;

        public static void init(string filename) {
            if (dict != null)
                return;
            dict = new Dictionary<int, ActionRecord>();

            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                ) {
                String[] toks;
                byte action_index;
                ActionRecord record;
                ActionRequirement action_req;

                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;
                    int index = int.Parse(toks[col["Type"]]);

                    if (!dict.TryGetValue(index, out record)) {
                        record = new ActionRecord();
                        record.list = new List<ActionRequirement>();
                        dict[index] = record;
                    }
                    if ((action_index = byte.Parse(toks[col["Index"]])) == 0)
                        record.max = byte.Parse(toks[col["Max"]]);
                    else {
                        action_req = new ActionRequirement();
                        action_req.index = action_index;
                        action_req.type = (ActionType) Enum.Parse(typeof (ActionType), toks[col["Action"]], true);
                        action_req.max = byte.Parse(toks[col["Max"]]);
                        action_req.parms = new string[toks.Length - 4];
                        for (int i = 4; i < toks.Length; ++i) {
                            if (toks[i].Contains("="))
                                action_req.parms[i - 4] = toks[i].Split('=')[1];
                            else
                                action_req.parms[i - 4] = toks[i];
                        }
                        if (!uint.TryParse(toks[col["EffectReq"]], out action_req.effectReqId))
                            action_req.effectReqId = 0;
                        if (toks[col["EffectReqInherit"]].Length > 0) {
                            action_req.effectReqInherit =
                                (EffectInheritance)
                                Enum.Parse(typeof (EffectInheritance), toks[col["EffectReqInherit"]], true);
                        } else
                            action_req.effectReqInherit = EffectInheritance.All;
                        record.list.Add(action_req);
                    }
                }
            }
        }

        public static ActionRecord getActionRequirementRecord(int workerID) {
            if (dict == null)
                return null;
            ActionRecord record;
            if (dict.TryGetValue(workerID, out record))
                return record;
            return null;
        }
    }
}