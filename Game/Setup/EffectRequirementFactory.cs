#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Setup {
    public class EffectRequirementFactory {
        private static Dictionary<uint, EffectRequirementContainer> dict;

        public static void init(string filename) {
            if (dict != null)
                return;
            dict = new Dictionary<uint, EffectRequirementContainer>();

            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                ) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                EffectRequirementContainer container;
                EffectRequirement req;
                Type type = typeof (RequirementFormula);
                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;

                    uint index = uint.Parse(toks[col["Id"]]);
                    if (!dict.TryGetValue(index, out container)) {
                        container = new EffectRequirementContainer();
                        container.ID = index;
                        dict.Add(index, container);
                    }
                    //string name = "Game.Logic.EffectRequirements." + toks[col["Method"]];
                    req = new EffectRequirement();
                    string[] parms = new string[toks.Length - 2];
                    for (int i = 2; i < toks.Length; ++i) {
                        if (toks[i].Contains("="))
                            parms[i - 2] = toks[i].Split('=')[1];
                        else
                            parms[i - 2] = toks[i];
                    }
                    req.parms = parms;
                    req.method = type.GetMethod(toks[col["Method"]]);
                    req.description = toks[col["Description"]];
                    container.add(req);
                }
            }
        }

        public static EffectRequirementContainer getEffectRequirementContainer(uint index) {
            if (dict == null)
                return null;
            EffectRequirementContainer tmp;
            if (dict.TryGetValue(index, out tmp))
                return tmp;
            return new EffectRequirementContainer();
        }
    }
}