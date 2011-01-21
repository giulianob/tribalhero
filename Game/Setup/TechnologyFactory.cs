#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Setup {
    public class TechnologyFactory {
        private static bool initc;

        private static Dictionary<uint, TechnologyBase> technologies = new Dictionary<uint, TechnologyBase>();
        private static Resource GetResource(int lvl, int buildType, int buildLvl) {
            if (lvl == 0) return new Resource();
            Resource ret = StructureFactory.GetCost(buildType, buildLvl + lvl-1) / 3;
            ret /= 10;
            return ret * 10;
        }
        private static int GetTime(int lvl, int buildType, int buildLvl) {
            if (lvl == 0) return 0;
            return (int)(StructureFactory.GetTime((ushort)buildType, (byte)(buildLvl + lvl - 1)) / 2);
        }
        public static void Init(string technologyFilename, string technologyEffectsFilename) {
            if (initc)
                return;

            initc = true;

            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(technologyFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                Dictionary<string, int> col = new Dictionary<string, int>();
                String[] toks;

                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;

                    TechnologyBase tech = new TechnologyBase {
                                                                 techtype = uint.Parse(toks[col["TechType"]]),
                                                                 name = toks[col["Name"]],
                                                                 level = byte.Parse(toks[col["Lvl"]]),
                                                                 time = (uint)(GetTime(byte.Parse(toks[col["Lvl"]]),int.Parse(toks[col["BuildType"]]),int.Parse(toks[col["BuildLvl"]]))),
                                                                 resources = GetResource( byte.Parse(toks[col["Lvl"]]),int.Parse(toks[col["BuildType"]]),int.Parse(toks[col["BuildLvl"]])),
                                                                 effects = new List<Effect>()
                                                             };

                    technologies[tech.techtype*100 + tech.level] = tech;
                }
            }

            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(technologyEffectsFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                Dictionary<string, int> col = new Dictionary<string, int>();
                String[] toks;
                TechnologyBase tech;
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;

                    if (!technologies.TryGetValue(uint.Parse(toks[col["TechType"]])*100 + byte.Parse(toks[col["Lvl"]]), out tech))
                        continue;

                    Effect effect = new Effect {id = (EffectCode) Enum.Parse(typeof (EffectCode), toks[col["Effect"]], true), isPrivate = bool.Parse(toks[col["IsPrivate"]])};
                    for (int i = 0; i < 5; ++i) {
                        string str = toks[col[string.Format("P{0}", i + 1)]];
                        int tmp;
                        if (str.StartsWith("{")) {
                            string name = str.Substring(1, str.IndexOf(':')-1);
                            string condition = str.Substring(str.IndexOf(':') + 1);
                            condition = condition.Remove(condition.LastIndexOf('}'));
                            // effect.value[i] = ConditionFactory.CreateICondition(name, condition);
                        } else if (int.TryParse(str, out tmp)) {
                            effect.value[i] = tmp;
                        } else {
                            effect.value[i] = str;
                        }
                    }

                    effect.location = (EffectLocation) Enum.Parse(typeof (EffectLocation), toks[col["Location"]], true);
                    tech.effects.Add(effect);
                }
            }
        }

        public static TechnologyBase GetTechnologyBase(uint type, byte level) {
            TechnologyBase ret;
            technologies.TryGetValue(type*100 + level, out ret);
            return ret;
        }

        public static Technology GetTechnology(uint type, byte level) {
            TechnologyBase tbase = GetTechnologyBase(type, level);
            if (tbase == null)
                return null;
            Technology t = new Technology(tbase);
            return t;
        }
    }
}