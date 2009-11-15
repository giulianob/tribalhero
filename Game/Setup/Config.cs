using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Game.Setup {
    public class Config {

        public static readonly int PORT = 48888;
        public static readonly string ADDRESS = "0.0.0.0";

        public static readonly uint map_width = 3808;
        public static readonly uint map_height = 6944;
        public static readonly uint region_width = 34;
        public static readonly uint region_height = 62;

        public static readonly int column = (int)(map_width / region_width);
        public static readonly int row = (int)(map_height / region_height);
        public static readonly int regions_count = column * row;

        public static readonly uint city_region_width = 56;
        public static readonly uint city_region_height = 56;
        public static readonly int city_region_column = (int)(map_width / city_region_width);
        public static readonly int city_region_row = (int)(map_height / city_region_height);

        public static readonly double seconds_per_unit = 0.01; //dont make it zero
        public static readonly int battle_turn_interval = 2;
        public static readonly int stamina_initial = 0;
        public static readonly bool resource_upkeep = false;
        public static readonly bool resource_cap = true;
        public static readonly bool resource_fast_income = true;

        public static readonly byte minimum_distance_apart = 8;
        public static readonly int height_margin = 10;
        public static readonly int width_margin = 10;

        public static readonly bool database_verbose = false;
        public static readonly bool database_empty = false;
        public static readonly bool database_load_players = true;
        public static readonly string database_host = "";
        public static readonly string database_username = "";
        public static readonly string database_password = "";
        public static readonly string database_database = "";
        public static readonly string database_salt = "";

        public static readonly bool ai_enabled = true;

        public static Random Random = new Random();

        static Config() {
            string key = string.Empty;

            try {
                using (StreamReader file = new StreamReader(File.Open("settings.ini", FileMode.Open))) {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (line == string.Empty) continue;

                        key = line.Substring(0, line.IndexOf('=')).ToLower();
                        key = key.Substring(0, key.IndexOf('.')) + "_" + key.Substring(key.IndexOf('.') + 1);

                        string value = line.Substring(line.IndexOf('=') + 1);

                        Type type = Type.GetType("Game.Setup.Config");
                        FieldInfo field = type.GetField(key, BindingFlags.Public | BindingFlags.Static);
                        if (field.FieldType.FullName == "System.Boolean")
                            field.SetValue(null, Boolean.Parse(value));
                        else if (field.FieldType.FullName == "System.Double")
                            field.SetValue(null, Double.Parse(value));
                        else if (field.FieldType.FullName == "System.Int32")
                            field.SetValue(null, Int32.Parse(value));
                        else if (field.FieldType.FullName == "System.UInt32")
                            field.SetValue(null, UInt32.Parse(value));
                        else
                            field.SetValue(null, value);
                    }
                }
            }
            catch (Exception e) {
                Global.Logger.Error("Error loading settings file at " + key, e);
            }
        }
    }
}
