#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Game.Data;
using Game.Logic;
using Game.Util;
using System.Linq;
#endregion

namespace Game.Setup
{

    public class ActionRecord
    {
        public int id;
        public byte max;
        public List<ActionRequirement> list;
    }

    public class ActionFactory : IEnumerable<ActionRecord> 
    {
        private static Dictionary<int, ActionRecord> dict;

        public static void Init(string filename)
        {
            if (dict != null)
                return;

            dict = new Dictionary<int, ActionRecord> {{0, new ActionRecord {id = 0, list = new List<ActionRequirement>(), max = 0}}};

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
                    int lvl = int.Parse(toks[col["Level"]]);
                    int type = int.Parse(toks[col["Type"]]);
                    int index = type * 100 + lvl;
                   
                    if (dict.Any(x => x.Key > index && x.Key < type * 100 + 99)) {
                        throw new Exception("Action out of sequence, newer lvl is found!");
                    }

                    int lastLvl = dict.Keys.LastOrDefault(x => x <= index && x > type * 100);
                    
                    if (lastLvl == 0) {
                        record = new ActionRecord { list = new List<ActionRequirement>(), id=index };
                        dict[index] = record;
                    } else if (lastLvl == index) {
                        record = dict[index];
                    } else {
                        ActionRecord lastActionRecord = dict[lastLvl];
                        record = new ActionRecord{ max=lastActionRecord.max, list = new List<ActionRequirement>(), id=index };
                        record.list.AddRange(lastActionRecord.list);
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
                        if (record.list.Any(x => x.index == action_index))
                        {
                            record.list.RemoveAll(x => x.index == action_index);
                        }
                        record.list.Add(actionReq);
                    }
                }
            }
        }

        public static ActionRecord GetActionRequirementRecordBestFit(int type, byte lvl) {
            if (dict == null)
                return null;

            int index = type * 100 + lvl;
            int lastLvl = dict.Keys.LastOrDefault(x => x <= type * 100 + lvl && x > type * 100);
                    
            if (lastLvl == 0) {
                Global.Logger.InfoFormat("WorkerID not found for [{0}][{1}]",type,lvl);
            }
            return dict[lastLvl];
        }

        public static ActionRecord GetActionRequirementRecord(int workerId)
        {
            if (dict == null)
                return null;

            ActionRecord record;
            if (!dict.TryGetValue(workerId, out record))
            {
                throw new Exception("Action Requirement Not Found!");
            }
            return record;
        }

        #region IEnumerable<ActionRecord> Members

        public IEnumerator<ActionRecord> GetEnumerator()
        {
            return ((IEnumerable<ActionRecord>)dict.Values).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dict.Values.GetEnumerator();
        }

        #endregion
    }
}