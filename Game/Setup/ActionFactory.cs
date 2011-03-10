#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Logic;
using Game.Util;
using System.Linq;
#endregion

namespace Game.Setup
{
    public class ActionRecord
    {
        public int Id { get; set; }
        public List<ActionRequirement> List { get; set; }
        public byte Max { get; set; }
    }

    public class ActionFactory : IEnumerable<ActionRecord>
    {
        private static Dictionary<int, ActionRecord> dict;

        public static void Init(string filename)
        {
            if (dict != null)
                return;

            dict = new Dictionary<int, ActionRecord> {{0, new ActionRecord {Id = 0, List = new List<ActionRequirement>(), Max = 0}}};

            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                ActionRecord record;
                ActionRequirement actionReq;

                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;
                    int lvl = int.Parse(toks[col["Level"]]);
                    int type = int.Parse(toks[col["Type"]]);
                    int index = type*100 + lvl;

                    if (dict.Any(x => x.Key > index && x.Key < type*100 + 99))
                    {
                        throw new Exception("Action out of sequence, newer lvl is found!");
                    }

                    int lastLvl = dict.Keys.LastOrDefault(x => x <= index && x > type*100);

                    if (lastLvl == 0)
                    {
                        record = new ActionRecord {List = new List<ActionRequirement>(), Id = index};
                        dict[index] = record;
                    }
                    else if (lastLvl == index)
                    {
                        record = dict[index];
                    }
                    else
                    {
                        ActionRecord lastActionRecord = dict[lastLvl];
                        record = new ActionRecord {Max = lastActionRecord.Max, List = new List<ActionRequirement>(), Id = index};
                        record.List.AddRange(lastActionRecord.List);
                        dict[index] = record;
                    }

                    byte actionIndex;
                    if ((actionIndex = byte.Parse(toks[col["Index"]])) == 0)
                        record.Max = byte.Parse(toks[col["Max"]]);
                    else
                    {
                        // Create action and set basic options
                        actionReq = new ActionRequirement
                                    {
                                            Index = actionIndex,
                                            Type = (ActionType)Enum.Parse(typeof(ActionType), (toks[col["Action"]] + "_active").ToCamelCase(), true),
                                            Max = byte.Parse(toks[col["Max"]])
                                    };

                        // Set action options
                        if (toks[col["Option"]].Length > 0)
                        {
                            foreach (var opt in toks[col["Option"]].Split('|'))
                                actionReq.Option |= (ActionOption)Enum.Parse(typeof(ActionOption), opt, true);
                        }

                        // Set action params
                        actionReq.Parms = new string[5];
                        for (int i = 6; i < 11; ++i)
                            actionReq.Parms[i - 6] = toks[i].Contains("=") ? toks[i].Split('=')[1] : toks[i];

                        // Set effect requirements
                        uint effectReqId;
                        actionReq.EffectReqId = uint.TryParse(toks[col["EffectReq"]], out effectReqId) ? effectReqId : 0;
                        if (toks[col["EffectReqInherit"]].Length > 0)
                            actionReq.EffectReqInherit = (EffectInheritance)Enum.Parse(typeof(EffectInheritance), toks[col["EffectReqInherit"]], true);
                        else
                            actionReq.EffectReqInherit = EffectInheritance.All;
                        if (record.List.Any(x => x.Index == actionIndex))
                            record.List.RemoveAll(x => x.Index == actionIndex);
                        
                        record.List.Add(actionReq);
                    }
                }
            }
        }

        public static ActionRecord GetActionRequirementRecordBestFit(int type, byte lvl)
        {
            if (dict == null)
                return null;

            int lastLvl = dict.Keys.LastOrDefault(x => x <= type * 100 + lvl && x > type * 100);

            if (lastLvl == 0)
                Global.Logger.InfoFormat("WorkerID not found for [{0}][{1}]", type, lvl);

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