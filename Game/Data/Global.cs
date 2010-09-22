#region

using System.Collections.Generic;
using Game.Database;
using Game.Logic;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using log4net;

#endregion

namespace Game.Data {
    public class Global {
        /// <summary>
        /// List of global locks. These should all have negative ids so they do not conflict with player locks.
        /// </summary>
        public enum Locks {
            FOREST = -1
        }

        public static readonly ILog Logger = LogManager.GetLogger(typeof(Global));
        public static readonly ILog DbLogger = LogManager.GetLogger(typeof(IDbManager));

        public static readonly IDbManager DbManager = new MySqlDbManager(Config.database_host, Config.database_username,
                                                                Config.database_password, Config.database_database);

        public static readonly AI Ai = new AI();

        public static readonly Scheduler Scheduler = new Scheduler();
        public static readonly Channel Channel = new Channel();

        public static Dictionary<uint, Player> Players { get; private set; }

        public static ForestManager Forests { get; private set; }

        public static World World { get; private set; }

        public static Dictionary<string, SystemVariable> SystemVariables { get; private set; }

        public static bool FireEvents { get; set; }

        static Global() {
            FireEvents = true;
            SystemVariables = new Dictionary<string, SystemVariable>();
            World = new World();
            Players = new Dictionary<uint, Player>();
            Forests = new ForestManager();
        }
    }
}