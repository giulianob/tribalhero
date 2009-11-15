using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Logic;
using System.IO;

namespace Game.Setup {
    public class Factory {
        internal static void BuildFiles(string output_file, string dir, string pattern) {
            BuildFiles2(output_file,Directory.GetFiles(dir, pattern));
        }

        static void BuildFiles2(string output_file, params string[] files) {
            if (File.Exists(output_file)) File.Delete(output_file);
            StreamWriter sw = new StreamWriter(output_file);
            bool header_written = false;
            foreach (string filename in files) {
                StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
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
            string csv_folder = "Game\\Setup\\CSV\\";
            string custom_folder = "Game\\Setup\\CSV\\Custom\\";

            BuildFiles(csv_folder + "structure.csv", custom_folder, "*structure.csv");
            StructureFactory.init("Game\\Setup\\CSV\\structure.csv");

            BuildFiles(csv_folder + "action.csv", custom_folder, "*action.csv");
            ActionFactory.init("Game\\Setup\\CSV\\action.csv");

            BuildFiles(csv_folder + "effect_requirement.csv", custom_folder, "*effect_requirement.csv");
            EffectRequirementFactory.init("Game\\Setup\\CSV\\effect_requirement.csv");

            BuildFiles(csv_folder + "init.csv", custom_folder, "*init.csv");
            InitFactory.init("Game\\Setup\\CSV\\init.csv");

            BuildFiles(csv_folder + "property.csv", custom_folder, "*property.csv");
            PropertyFactory.init("Game\\Setup\\CSV\\property.csv");

            BuildFiles(csv_folder + "layout.csv", custom_folder, "*layout.csv");
            ReqirementFactory.init("Game\\Setup\\CSV\\layout.csv");

            BuildFiles(csv_folder + "technology.csv", custom_folder, "*technology.csv");
            BuildFiles(csv_folder + "technology_effects.csv", custom_folder, "*technology_effects.csv");
            TechnologyFactory.init("Game\\Setup\\CSV\\technology.csv", "Game\\Setup\\CSV\\technology_effects.csv");

            BuildFiles(csv_folder + "unit.csv", custom_folder, "*unit.csv");
            UnitFactory.init("Game\\Setup\\CSV\\unit.csv");

            File.Copy("Game\\Setup\\CSV\\Custom\\object_type.csv", "Game\\Setup\\CSV\\object_type.csv", true);
            ObjectTypeFactory.init("Game\\Setup\\CSV\\object_type.csv");

            MapFactory.init("CityLocations.txt");
        }
    }
}
