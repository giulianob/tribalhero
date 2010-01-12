#region

using System.IO;
using Game.Data;

#endregion

namespace Game.Setup {
    public class Factory {
        internal static void BuildFiles(string output_file, string dir, string pattern) {
            Global.Logger.Info("Building CSV file " + output_file);
            BuildFiles2(output_file, Directory.GetFiles(dir, pattern));
        }

        private static void BuildFiles2(string output_file, params string[] files) {
            if (File.Exists(output_file))
                File.Delete(output_file);
            StreamWriter sw = new StreamWriter(output_file);
            bool header_written = false;
            foreach (string filename in files) {
                StreamReader sr =
                    new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                string header = sr.ReadLine();
                if (!header_written) {
                    sw.WriteLine(header);
                    header_written = true;
                }
                sw.Write(sr.ReadToEnd());
                sr.Close();
            }
            sw.Close();
        }

        public static void initAll() {
            BuildFiles(Config.csv_compiled_folder + "structure.csv", Config.csv_folder, "*structure.csv");
            StructureFactory.init(Config.csv_compiled_folder + "structure.csv");

            BuildFiles(Config.csv_compiled_folder + "action.csv", Config.csv_folder, "*action.csv");
            ActionFactory.init(Config.csv_compiled_folder + "action.csv");

            BuildFiles(Config.csv_compiled_folder + "effect_requirement.csv", Config.csv_folder,
                       "*effect_requirement.csv");
            EffectRequirementFactory.init(Config.csv_compiled_folder + "effect_requirement.csv");

            BuildFiles(Config.csv_compiled_folder + "init.csv", Config.csv_folder, "*init.csv");
            InitFactory.init(Config.csv_compiled_folder + "init.csv");

            BuildFiles(Config.csv_compiled_folder + "property.csv", Config.csv_folder, "*property.csv");
            PropertyFactory.init(Config.csv_compiled_folder + "property.csv");

            BuildFiles(Config.csv_compiled_folder + "layout.csv", Config.csv_folder, "*layout.csv");
            ReqirementFactory.init(Config.csv_compiled_folder + "layout.csv");

            BuildFiles(Config.csv_compiled_folder + "technology.csv", Config.csv_folder, "*technology.csv");
            BuildFiles(Config.csv_compiled_folder + "technology_effects.csv", Config.csv_folder,
                       "*technology_effects.csv");
            TechnologyFactory.init(Config.csv_compiled_folder + "technology.csv",
                                   Config.csv_folder + "technology_effects.csv");

            BuildFiles(Config.csv_compiled_folder + "unit.csv", Config.csv_folder, "*unit.csv");
            UnitFactory.init(Config.csv_compiled_folder + "unit.csv");

            File.Copy(Config.csv_folder + "object_type.csv", Config.csv_compiled_folder + "object_type.csv", true);
            ObjectTypeFactory.init(Config.csv_compiled_folder + "object_type.csv");

            MapFactory.init(Config.settings_folder + "CityLocations.txt");
        }
    }
}