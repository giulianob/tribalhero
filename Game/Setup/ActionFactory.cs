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

    public class ActionRecord
    {
        public byte max;
        public List<ActionRequirement> list;
    }

    public class ActionFactory
    {
        private static Dictionary<int, ActionRecord> dict;

        public static void Init(string filename)
        {
            if (dict != null)
                return;

            dict = new Dictionary<int, ActionRecord>();

            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                )
            {
                String[] toks;
                byte action_index;
                ActionRecord record;
                ActionRequirement actionReq;

                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;
                    int index = int.Parse(toks[col["Type"]]);

                    if (!dict.TryGetValue(index, out record))
                    {
                        record = new ActionRecord { list = new List<ActionRequirement>() };
                        dict[index] = record;
                    }
                    if ((action_index = byte.Parse(toks[col["Index"]])) == 0)
                        record.max = byte.Parse(toks[col["Max"]]);
                    else
                    {
                        // Create action and set basic options
                        actionReq = new ActionRequirement { index = action_index, type = (ActionType)Enum.Parse(typeof(ActionType), toks[col["Action"]], true), max = byte.Parse(toks[col["Max"]]) };

                        // Set action options
                        if (toks[col["Option"]].Length > 0)
                        {
                            foreach (string opt in toks[col["Option"]].Split('|'))
                                actionReq.option |= (ActionOption)Enum.Parse(typeof(ActionOption), opt, true);
                        }

                        // Set action params
                        actionReq.parms = new string[5];
                        for (int i = 5; i < 10; ++i)
                        {
                            actionReq.parms[i - 5] = toks[i].Contains("=") ? toks[i].Split('=')[1] : toks[i];
                        }

                        // Set effect requirements
                        if (!uint.TryParse(toks[col["EffectReq"]], out actionReq.effectReqId))
                            actionReq.effectReqId = 0;
                        if (toks[col["EffectReqInherit"]].Length > 0)
                        {
                            actionReq.effectReqInherit = (EffectInheritance)Enum.Parse(typeof(EffectInheritance), toks[col["EffectReqInherit"]], true);
                        }
                        else
                        {
                            actionReq.effectReqInherit = EffectInheritance.ALL;
                        }

                        record.list.Add(actionReq);
                    }
                }
            }
        }

        public static ActionRecord GetActionRequirementRecord(int workerId)
        {
            if (dict == null)
                return null;

            ActionRecord record;
            return dict.TryGetValue(workerId, out record) ? record : null;
        }
    }
}