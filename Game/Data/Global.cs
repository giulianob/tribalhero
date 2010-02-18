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
        public static ILog Logger = LogManager.GetLogger(typeof (Global));
        public static ILog DbLogger = LogManager.GetLogger(typeof (IDbManager));

        public static IDbManager dbManager = new MySqlDbManager(Config.database_host, Config.database_username,
                                                                Config.database_password, Config.database_database);

        public static AI AI = new AI();
        public static Scheduler Scheduler = new Scheduler();
        public static Channel Channel = new Channel();

        private static Dictionary<uint, Player> players = new Dictionary<uint, Player>();

        private static World world = new World();

        private static Dictionary<string, SystemVariable> systemVariables = new Dictionary<string, SystemVariable>();

        private static bool fireEvents = true;

        public static bool FireEvents {
            get { return fireEvents; }
        }

        private Global() {}

        public static Dictionary<uint, Player> Players {
            get { return players; }
        }

        public static World World {
            get { return world; }
        }

        public static Dictionary<string, SystemVariable> SystemVariables {
            get { return systemVariables; }
        }

        public static void PauseEvents() {
            fireEvents = false;
        }

        public static void ResumeEvents() {
            fireEvents = true;
        }
    }
}