#region

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Game.Logic;

#endregion

namespace Game.Setup
{
    public class EffectRequirementFactory
    {
        private readonly Dictionary<uint, EffectRequirementContainer> dict = new Dictionary<uint, EffectRequirementContainer>();

        public void Init(string filename)
        {            
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

                Type type = typeof(RequirementFormula);
                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                    {
                        continue;
                    }

                    uint index = uint.Parse(toks[col["Id"]]);
                    EffectRequirementContainer container;
                    if (!dict.TryGetValue(index, out container))
                    {
                        container = new EffectRequirementContainer {Id = index};
                        dict.Add(index, container);
                    }
                    //string name = "Game.Logic.EffectRequirements." + toks[col["Method"]];
                    EffectRequirement req = new EffectRequirement();
                    var parms = new string[toks.Length - 2];
                    for (int i = 2; i < toks.Length; ++i)
                    {
                        if (toks[i].Contains("="))
                        {
                            parms[i - 2] = toks[i].Split('=')[1];
                        }
                        else
                        {
                            parms[i - 2] = toks[i];
                        }
                    }

                    req.Parms = parms;
                    req.Method = type.GetMethod(toks[col["Method"]]);
                    req.Description = toks[col["Description"]].Trim();
                    req.WebsiteDescription = toks[col["Website Description"]].Trim();

                    if (req.Method == null)
                    {
                        throw new Exception(string.Format("Could not find effect requirement method {0}", toks[col["Method"]]));
                    }

                    container.Add(req);
                }
            }
        }

        public EffectRequirementContainer GetEffectRequirementContainer(uint index)
        {
            if (dict == null)
            {
                return null;
            }
            EffectRequirementContainer tmp;
            if (dict.TryGetValue(index, out tmp))
            {
                return tmp;
            }
            return new EffectRequirementContainer();
        }
    }
}