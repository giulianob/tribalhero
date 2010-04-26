#region

using System.IO;
using Game.Data;

#endregion

namespace Game.Setup {
    public class Factory {
        internal static void BuildFiles(string outputFile, string dir, string pattern) {
            Global.Logger.Info("Building CSV file " + outputFile);
            BuildFiles2(outputFile, Directory.GetFiles(dir, pattern));
        }

        private static void BuildFiles2(string outputFile, params string[] files) {
            if (File.Exists(outputFile))
                File.Delete(outputFile);
            StreamWriter sw = new StreamWriter(outputFile);
            bool headerWritten = false;
            foreach (string filename in files) {
                StreamReader sr =
                    new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                string header = sr.ReadLine();
                if (!headerWritten) {
                    sw.WriteLine(header);
                    headerWritten = true;
                }
                sw.Write(sr.ReadToEnd());
                sr.Close();
            }
            sw.Close();
        }

        public static void CompileConfigFiles() {
            BuildFiles(Config.csv_compiled_folder + "structure.csv", Config.csv_folder, "*structure.csv");
            BuildFiles(Config.csv_compiled_folder + "action.csv", Config.csv_folder, "*action.csv");
            BuildFiles(Config.csv_compiled_folder + "effect_requirement.csv", Config.csv_folder, "*effect_requirement.csv");
            BuildFiles(Config.csv_compiled_folder + "init.csv", Config.csv_folder, "*init.csv");
            BuildFiles(Config.csv_compiled_folder + "property.csv", Config.csv_folder, "*property.csv");
            BuildFiles(Config.csv_compiled_folder + "layout.csv", Config.csv_folder, "*layout.csv");
            BuildFiles(Config.csv_compiled_folder + "technology.csv", Config.csv_folder, "*technology.csv");
            BuildFiles(Config.csv_compiled_folder + "technology_effects.csv", Config.csv_folder, "*technology_effects.csv");
            BuildFiles(Config.csv_compiled_folder + "unit.csv", Config.csv_folder, "*unit.csv");
            File.Copy(Config.csv_folder + "object_type.csv", Config.csv_compiled_folder + "object_type.csv", true);
        }

        public static void InitAll() {
            StructureFactory.Init(Config.csv_compiled_folder + "structure.csv");
            ActionFactory.init(Config.csv_compiled_folder + "action.csv");
            EffectRequirementFactory.init(Config.csv_compiled_folder + "effect_requirement.csv");
            InitFactory.Init(Config.csv_compiled_folder + "init.csv");
            PropertyFactory.Init(Config.csv_compiled_folder + "property.csv");
            ReqirementFactory.Init(Config.csv_compiled_folder + "layout.csv");
            TechnologyFactory.Init(Config.csv_compiled_folder + "technology.csv", Config.csv_folder + "technology_effects.csv");
            UnitFactory.Init(Config.csv_compiled_folder + "unit.csv");
            ObjectTypeFactory.init(Config.csv_compiled_folder + "object_type.csv");
            MapFactory.init(Config.settings_folder + "CityLocations.txt");
        }
    }
}