#region

using System.IO;
using Game.Data;

#endregion

namespace Game.Setup
{
    public class Factory
    {
        internal static void BuildFiles(string outputFile, string dir, string pattern)
        {
            outputFile = Path.GetFullPath(outputFile);
            Global.Logger.Info("Building CSV file " + outputFile);

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            using (var sw = new StreamWriter(outputFile))
            {
                bool headerWritten = false;
                foreach (var filename in Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                {
                    string fullFilename = filename;

                    if (Global.IsRunningOnMono())
                        fullFilename = Path.Combine(dir, filename);

                    using (var sr = new StreamReader(new FileStream(fullFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        string header = sr.ReadLine();
                        if (!headerWritten)
                        {
                            sw.WriteLine(header);
                            headerWritten = true;
                        }
                        sw.Write(sr.ReadToEnd());
                    }
                }
            }
        }

        public static void CompileConfigFiles()
        {
            Directory.CreateDirectory(Config.csv_compiled_folder);

            BuildFiles(Path.Combine(Config.csv_compiled_folder, "structure.csv"), Config.csv_folder, "*structure.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "action.csv"), Config.csv_folder, "*action.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "effect_requirement.csv"), Config.csv_folder, "*effect_requirement.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "init.csv"), Config.csv_folder, "*init.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "property.csv"), Config.csv_folder, "*property.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "layout.csv"), Config.csv_folder, "*layout.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "technology.csv"), Config.csv_folder, "*technology.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "technology_effects.csv"), Config.csv_folder, "*technology_effects.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "unit.csv"), Config.csv_folder, "*unit.csv");
            File.Copy(Path.Combine(Config.csv_folder, "object_type.csv"), Path.Combine(Config.csv_compiled_folder, "object_type.csv"), true);
        }

        public static void InitAll()
        {
            StructureFactory.Init(Path.Combine(Config.csv_compiled_folder, "structure.csv"));
            ActionFactory.Init(Path.Combine(Config.csv_compiled_folder, "action.csv"));
            EffectRequirementFactory.Init(Path.Combine(Config.csv_compiled_folder, "effect_requirement.csv"));
            InitFactory.Init(Path.Combine(Config.csv_compiled_folder, "init.csv"));
            PropertyFactory.Init(Path.Combine(Config.csv_compiled_folder, "property.csv"));
            RequirementFactory.Init(Path.Combine(Config.csv_compiled_folder, "layout.csv"));
            TechnologyFactory.Init(Path.Combine(Config.csv_compiled_folder, "technology.csv"), Path.Combine(Config.csv_folder, "technology_effects.csv"));
            UnitFactory.Init(Path.Combine(Config.csv_compiled_folder, "unit.csv"));
            ObjectTypeFactory.Init(Path.Combine(Config.csv_compiled_folder, "object_type.csv"));
            MapFactory.Init(Path.Combine(Config.maps_folder, "CityLocations.txt"));
        }
    }
}