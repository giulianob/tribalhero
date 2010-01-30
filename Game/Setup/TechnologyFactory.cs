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
        //        static Dictionary<uint, List<TechnologyRecord>> technologiesBase = new Dictionary<uint, List<TechnologyRecord>>();

        //        static Dictionary<uint, List<TechnologyEffectRecord>> technologiesEffectByTechType = new Dictionary<uint, List<TechnologyEffectRecord>>();
        //        static Dictionary<uint, TechnologyEffectRecord> technologiesEffectByTechTypeAndLevel = new Dictionary<uint, TechnologyEffectRecord>();

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
                                                                 level = byte.Parse(toks[col["Lvl"]]),
                                                                 time = ushort.Parse(toks[col["Time"]]),
                                                                 resources =
                                                                     new Resource(int.Parse(toks[col["Crop"]]), int.Parse(toks[col["Gold"]]), int.Parse(toks[col["Iron"]]), int.Parse(toks[col["Wood"]]),
                                                                                  int.Parse(toks[col["Labor"]])),
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
                        if (int.TryParse(str, out tmp))
                            effect.value[i] = tmp;
                        else
                            effect.value[i] = str;
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