#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Setup
{
    public class TechnologyFactory
    {
        private static bool initc;

        private static readonly Dictionary<uint, TechnologyBase> technologies = new Dictionary<uint, TechnologyBase>();
		
        private static Resource GetResource(int lvl, int buildType, int buildLvl) 
        {
            if (lvl == 0) return new Resource();
            Resource ret = StructureFactory.GetCost(buildType, buildLvl) * 1 / 3;
            ret /= 10;
            return ret * 10;
        }
        
		private static int GetTime(int lvl, int buildType, int buildLvl) 
        {
            if (lvl == 0) return 0;
            return StructureFactory.GetTime((ushort)buildType, (byte)buildLvl) * 2 / 3;
        }
		
        public static void Init(string technologyFilename, string technologyEffectsFilename)
        {
            if (initc)
                return;

            initc = true;

            using (var reader = new CsvReader(new StreamReader(new FileStream(technologyFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                var col = new Dictionary<string, int>();
                String[] toks;

                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;

                    var tech = new TechnologyBase
                               {
                                       Techtype = uint.Parse(toks[col["TechType"]]),
                                       Name = toks[col["Name"]],
                                       Level = byte.Parse(toks[col["Lvl"]]),
                                       Time = (uint)(GetTime(byte.Parse(toks[col["Lvl"]]),int.Parse(toks[col["BuildType"]]),int.Parse(toks[col["BuildLvl"]]))),
                                       Resources = GetResource( byte.Parse(toks[col["Lvl"]]),int.Parse(toks[col["BuildType"]]),int.Parse(toks[col["BuildLvl"]])),
                                       Effects = new List<Effect>()
                               };

                    technologies[tech.Techtype*100 + tech.Level] = tech;
                }
            }

            using (var reader = new CsvReader(new StreamReader(new FileStream(technologyEffectsFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                var col = new Dictionary<string, int>();
                String[] toks;
                TechnologyBase tech;
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;

                    if (!technologies.TryGetValue(uint.Parse(toks[col["TechType"]])*100 + byte.Parse(toks[col["Lvl"]]), out tech))
                        continue;

                    var effect = new Effect
                                 {Id = (EffectCode)Enum.Parse(typeof(EffectCode), toks[col["Effect"]], true), IsPrivate = bool.Parse(toks[col["IsPrivate"]])};

                    for (int i = 0; i < 5; ++i)
                    {
                        string str = toks[col[string.Format("P{0}", i + 1)]];
                        int tmp;
                        if (str.StartsWith("{"))
                        {
                        }
                        else if (int.TryParse(str, out tmp))
                            effect.Value[i] = tmp;
                        else
                            effect.Value[i] = str;
                    }

                    effect.Location = (EffectLocation)Enum.Parse(typeof(EffectLocation), toks[col["Location"]], true);
                    tech.Effects.Add(effect);
                }
            }
        }

        public static TechnologyBase GetTechnologyBase(uint type, byte level)
        {
            TechnologyBase ret;
            technologies.TryGetValue(type*100 + level, out ret);
            return ret;
        }

        public static Technology GetTechnology(uint type, byte level)
        {
            TechnologyBase tbase = GetTechnologyBase(type, level);
            if (tbase == null)
                return null;
            var t = new Technology(tbase);
            return t;
        }

        public static IEnumerable<TechnologyBase> AllTechnologies()
        {
            return technologies.Values;
        }
    }
}