#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Game.Data;
using JsonFx.Json;

#endregion

namespace Game.Setup
{
    public class Config
    {
        #region Game Settings

        public static double seconds_per_unit = 1.0;
        public static int[] forest_count = new[] {550, 750, 1200, 1000};
        public static bool ai_enabled;
        public static int ai_count = 100;

        #endregion        

        #region Environment Settings

        public static int client_min_version = 0;
        public static int client_min_revision = 0;
        public static int server_port = 48888;
        public static string server_listen_address = "0.0.0.0";
        public static bool server_admin_only;
        public static bool server_admin_always;
        public static int scheduler_threads = 25;
        public static bool server_production = true;
        public static string flash_domain = "tribalhero.com";
        public static string api_domain = "tribalhero.com";
        public static string api_id = string.Empty;
        public static string api_key = string.Empty;
        public static string csv_folder = "conf/csv/";
        public static string csv_compiled_folder = "conf/csv/compiled/";
        public static string maps_folder = "conf/maps/";
        public static string data_folder = "conf/data/";
        public static string regions_folder = "conf/regions/";
        public static bool locks_check;

        #endregion

        #region Map Settings

        public static uint map_width = 3400;
        public static uint map_height = 6200;
        public static uint region_width = 34;
        public static uint region_height = 62;
        public static int road_start_tile_id = 224;
        public static int road_end_tile_id = 255;
        public static uint city_region_width = 100;
        public static uint city_region_height = 100;
        public static int friend_invite_radius = 250;
        
        #endregion

        #region Database Settings

        public static readonly string database_schema_version = "20130427002657"; 
        public static bool database_verbose;
        public static bool database_empty;
        public static bool database_load_players = true;
        public static int database_timeout = 60;
        public static int database_max_connections = 50;
        public static string database_host = "127.0.0.1";
        public static string database_username = "root";
        public static string database_password = "";
        public static string database_database = "game";
        public static string database_salt = "DFjkxcVsDfwgf4kuj2sDmM334";

        #endregion

        #region Chat Settings
 
        public static string welcome_motd = string.Empty;
        public static PlayerRights chat_min_level = PlayerRights.Basic;

        #endregion

        #region Idle Settings

        public static bool players_remove_idle = true;
        public static int idle_days = 3;

        #endregion

        #region Id Ranges

        public static uint tribe_id_min = 1000000;
        public static uint tribe_id_max = 2999999;        
        public static uint stronghold_id_min = 3000000;
        public static uint stronghold_id_max = 3499999;
        public static uint barbariantribe_id_min = 3500000;
        public static uint barbariantribe_id_max = 3999999;
        public static uint forest_id_min = 4000000;
        public static uint forest_id_max = 4999999;
        public static uint city_id_min = 4000000;
        public static uint city_id_max = 4999999;      

        #endregion        

        #region Battle Settings

        public static int newbie_protection = 259200;
        public static bool battle_instant_watch;
        public static double battle_turn_interval = 20.0f;
        public static int battle_min_rounds = 5;
        public static int battle_retreat_min_rounds = 0;
        public static int battle_loot_till_full = 15; 
        public static int battle_loot_begin_round = 5;
        public static int battle_stamina_initial = 20;
        public static ushort battle_stamina_destroyed_deduction = 5;
        public static int battle_stamina_gate_multiplier = 1;
        public static double battle_cost_penalty = 1;
        public static int battle_loot_resource_crop_ratio = 1;
        public static int battle_loot_resource_labor_ratio = 1;
        public static int battle_loot_resource_wood_ratio = 1;
        public static int battle_loot_resource_gold_ratio = 2;
        public static int battle_loot_resource_iron_ratio = 5;

        #endregion

        #region Stronghold Settings

        public static int stronghold_generate = 250;
        public static int stronghold_activation_check_interval_in_sec = 3600;
        public static int stronghold_cities_per_level = 5;
        public static int stronghold_radius_per_level = 10;
        public static int stronghold_radius_base = 100;        
        public static double stronghold_npc_randomness = 0.4;
        public static int stronghold_fixed_upkeep = 0;
        public static int stronghold_gate_limit;
        public static int stronghold_battle_meter;
        public static bool stronghold_bypass_activation = false;       
        
        #endregion

        #region City Settings

        public static bool resource_cap = true;
        public static bool resource_fast_income;
        public static decimal ap_deduction_per_hour = .25m;
        public static decimal ap_max_per_battle = 4;
        public static bool troop_starve = true;

        #endregion

        #region Barbarian Tribe Settings

        public static int barbariantribe_generate = 2200;
        public static int barbariantribe_camp_count = 10;
        public static int barbariantribe_idle_check_interval_in_sec = 1800;
        public static int barbariantribe_idle_duration_in_sec = 86400 * 3;
        public static double barbarian_tribes_npc_randomness = 0.4;           

        #endregion

        #region Actions Settings
        
        public static bool actions_instant_time;
        public static bool actions_skip_city_actions;
        public static int actions_free_cancel_interval_in_sec = 60;
        public static bool actions_ignore_requirements;

        #endregion
        
        [ThreadStatic]
        private static Random random;

        public static Random Random
        {
            get
            {
                return random ?? (random = new Random());
            }
        }

        public static readonly Dictionary<string, string> ExtraProperties = new Dictionary<string, string>();        
        
        public static void LoadConfigFile(string settingsFile = null)
        {
            if (string.IsNullOrEmpty(settingsFile))
            {
                settingsFile = @"conf\settings.ini";
            }

            string key = string.Empty;

            try
            {
                settingsFile = Path.GetFullPath(settingsFile);

                using (var file = new StreamReader(File.Open(settingsFile, FileMode.Open, FileAccess.Read)))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (line == string.Empty || line.StartsWith("#") || line.StartsWith(";") || line.StartsWith("\\\\"))
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
                            ExtraProperties[key] = value;
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
                throw new Exception("Error loading settings file at " + key, e);
            }
        }
    }
}