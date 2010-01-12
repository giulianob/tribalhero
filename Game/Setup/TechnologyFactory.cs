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

        public static void init(string technology_filename, string technology_effects_filename) {
            if (initc)
                return;

            initc = true;

            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(technology_filename, FileMode.Open, FileAccess.Read,
                                                        FileShare.ReadWrite)))) {
                Dictionary<string, int> col = new Dictionary<string, int>();
                String[] toks;

                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;

                    TechnologyBase tech = new TechnologyBase();

                    tech.techtype = uint.Parse(toks[col["TechType"]]);
                    tech.level = byte.Parse(toks[col["Lvl"]]);
                    tech.time = ushort.Parse(toks[col["Time"]]);
                    tech.resources = new Resource(int.Parse(toks[col["Crop"]]), int.Parse(toks[col["Gold"]]),
                                                  int.Parse(toks[col["Iron"]]), int.Parse(toks[col["Wood"]]),
                                                  int.Parse(toks[col["Labor"]]));
                    tech.effects = new List<Effect>();

                    technologies[tech.techtype*100 + tech.level] = tech;
                }
            }

            using (
                CSVReader reader =
                    new CSVReader(
                        new StreamReader(new FileStream(technology_effects_filename, FileMode.Open, FileAccess.Read,
                                                        FileShare.ReadWrite)))) {
                Dictionary<string, int> col = new Dictionary<string, int>();
                String[] toks;
                TechnologyBase tech;
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;

                    if (technologies.TryGetValue(uint.Parse(toks[col["TechType"]])*100 + byte.Parse(toks[col["Lvl"]]),
                                                 out tech)) {
                        Effect effect = new Effect();
                        effect.id = (EffectCode) Enum.Parse(typeof (EffectCode), toks[col["Effect"]], true);
                        effect.isPrivate = bool.Parse(toks[col["IsPrivate"]]);
                        for (int i = 0; i < 5; ++i) {
                            string str = toks[col[string.Format("P{0}", i + 1)]];
                            int tmp;
                            if (int.TryParse(str, out tmp))
                                effect.value[i] = tmp;
                            else
                                effect.value[i] = str;
                        }

                        effect.location =
                            (EffectLocation) Enum.Parse(typeof (EffectLocation), toks[col["Location"]], true);
                        tech.effects.Add(effect);
                    }
                }
            }
        }

        public static TechnologyBase getTechnologyBase(uint type, byte level) {
            TechnologyBase ret = null;
            technologies.TryGetValue(type*100 + level, out ret);
            return ret;
        }

        public static Technology getTechnology(uint type, byte level) {
            TechnologyBase tbase = getTechnologyBase(type, level);
            if (tbase == null)
                return null;
            Technology t = new Technology(tbase);
            return t;
        }
    }
}