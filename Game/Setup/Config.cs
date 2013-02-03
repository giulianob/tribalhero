#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JsonFx.Json;
using log4net;
using log4net.Config;

#endregion

namespace Game.Setup
{
    public class Config
    {
        public static string database_schema_version = "20130202045301";

        // ReSharper disable InconsistentNaming        
        public static int client_min_version;

        public static int client_min_revision;

        public static int server_port = 48888;

        public static string server_listen_address = "0.0.0.0";

        public static bool server_admin_only;

        public static bool server_admin_always;

        public static bool server_production = true;

        public static bool players_remove_idle = true;

        public static string flash_domain = "tribalhero.com";

        public static string api_domain = "tribalhero.com";

        public static string api_id = string.Empty;

        public static string api_key = string.Empty;

        public static string csv_folder = "conf/csv/";

        public static string csv_compiled_folder = "conf/csv/compiled/";

        public static string settings_folder = "conf/";

        public static string maps_folder = "conf/maps/";

        public static string data_folder = "conf/data/";

        public static string regions_folder = "conf/regions/";

        public static uint map_width = 3400;

        public static uint map_height = 6200;

        public static uint region_width = 34;

        public static uint region_height = 62;

        public static int road_set_count = 1;

        public static int road_start_tile_id = 224;

        public static int road_end_tile_id = 255;

        public static int column = (int)(map_width / region_width);

        public static int row = (int)(map_height / region_height);

        public static int regions_count = column * row;

        public static uint city_region_width = 100;

        public static uint city_region_height = 100;

        public static int city_region_column = (int)(map_width / city_region_width);

        public static int city_region_row = (int)(map_height / city_region_height);

        public static double seconds_per_unit = 1.0; //dont make it zero!

        public static bool battle_instant_watch;

        public static double battle_turn_interval = 20.0f;

        public static int battle_min_rounds = 5;

        public static int battle_loot_till_full = 15;
                          // percentage of total carry, 15 round to fill up. This number cannot be set 10 or under.

        public static int battle_loot_begin_round = 5;

        public static int battle_stamina_initial = 20;

        public static ushort battle_stamina_destroyed_deduction = 5;

        public static int battle_stamina_gate_multiplier = 1;
        
        public static double battle_cost_penalty = 1;

        public static bool resource_cap = true;

        public static bool resource_fast_income;

        public static int resource_crop_ratio = 1;

        public static int resource_labor_ratio = 1;

        public static int resource_wood_ratio = 1;

        public static int resource_gold_ratio = 2;

        public static int resource_iron_ratio = 5;

        public static decimal ap_deduction_per_hour = .25m;

        public static decimal ap_max_per_battle = 4;

        public static bool troop_starve = true;

        public static byte minimum_distance_apart = 8;

        public static int height_margin = 10;

        public static int width_margin = 10;

        public static int[] forest_count = new[] {550, 750, 1200, 1000};

        public static int newbie_protection = 259200;
                          /* Number of seconds for newbie protection, set back to 3 days, which is more than enough
                                                       * 1) everyone around you should be the same lvls with the way we spawn.
                                                       * 2) low level skirmishers should begin shortly */

        public static bool database_verbose;

        public static bool database_empty;

        public static bool database_load_players = true;

        public static int database_timeout = 60;

        public static string database_host = "127.0.0.1";

        public static string database_username = "root";

        public static string database_password = "";

        public static string database_database = "game";

        public static string database_test = "game_test";

        public static string database_salt = "DFjkxcVsDfwgf4kuj2sDmM334";

        public static bool database_dump;

        public static bool ai_enabled;

        public static int ai_count = 100;

        public static bool actions_instant_time;

        public static bool actions_skip_city_actions;

        public static int actions_free_cancel_interval_in_sec = 60;

        public static bool actions_ignore_requirements;

        public static int barbariantribe_generate = 2200;

        public static int barbariantribe_camp_count = 10;

        public static int barbariantribe_idle_check_interval_in_sec = 1800;

        public static int barbariantribe_idle_duration_in_sec = 86400 * 3;

        public static int stronghold_generate = 250;

        public static int stronghold_activation_check_interval_in_sec = 3600;

        public static int stronghold_cities_per_level = 5;

        public static int stronghold_radius_per_level = 10;

        public static int stronghold_radius_base = 100;

        public static double barbarian_tribes_npc_randomness = 0.4;
        
        public static double stronghold_npc_randomness = 0.4;

        public static int stronghold_fixed_upkeep = 0;

        public static int stronghold_gate_limit;

        public static int stronghold_battle_meter;
        public static bool stronghold_bypass_activation = false;

        public static int idle_days = 3;

        public static string welcome_motd = string.Empty;

        [ThreadStatic]
        private static Random random;

        private static readonly Dictionary<string, string> extraProperties = new Dictionary<string, string>();

        public static Random Random
        {
            get
            {
                return random ?? (random = new Random());
            }
        }

        public static Dictionary<string, string> ExtraProperties
        {
            get
            {
                return extraProperties;
            }
        }

        // ReSharper restore InconsistentNaming

        public static void LoadConfigFile(string settingsFile = null)
        {
            if (string.IsNullOrEmpty(settingsFile))
            {
                settingsFile = @"conf\settings.ini";
            }

            XmlConfigurator.Configure();
            ILog logger = LogManager.GetLogger(typeof(Config));

            string key = string.Empty;

            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                logger.InfoFormat("Loading settings from {0}", settingsFile);

                using (var file = new StreamReader(File.Open(settingsFile, FileMode.Open, FileAccess.Read)))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (line == string.Empty || line.StartsWith("#") || line.StartsWith(";") ||
                            line.StartsWith("\\\\"))
                        {
                            continue;
                        }

                        key = line.Substring(0, line.IndexOf('=')).ToLower();
                        key = key.Substring(0, key.IndexOf('.')) + "_" + key.Substring(key.IndexOf('.') + 1).Trim();

                        string value = line.Substring(line.IndexOf('=') + 1).Trim();

                        Type type = Type.GetType("Game.Setup.Config");
                        FieldInfo field = type.GetField(key, BindingFlags.Public | BindingFlags.Static);

                        if (field == null)
                        {
                            extraProperties[key] = value;
                            continue;
                        }

                        switch(field.FieldType.FullName)
                        {
                            case "System.Boolean":
                                field.SetValue(null, Boolean.Parse(value));
                                break;
                            case "System.Double":
                                field.SetValue(null, Double.Parse(value));
                                break;
                            case "System.Int32":
                                field.SetValue(null, Int32.Parse(value));
                                break;
                            case "System.UInt32":
                                field.SetValue(null, UInt32.Parse(value));
                                break;
                            case "System.Int32[]":
                                field.SetValue(null, (new JsonReader()).Read<int[]>(value));
                                break;
                            case "System.Decimal":
                                field.SetValue(null, decimal.Parse(value));
                                break;
                            default:
                                field.SetValue(null, value);
                                break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error("Error loading settings file at " + key, e);
            }
        }
    }
}