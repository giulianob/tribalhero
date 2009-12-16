using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Data;
using log4net;
using Game.Database;
using System.Threading;
using Game.Util;
using Game.Setup;
using Game.Module;
using Game.Logic;

namespace Game {
    public class Global {
        static readonly Global instance = new Global();

        public static ILog Logger = LogManager.GetLogger(typeof(Global));
        public static ILog DbLogger = LogManager.GetLogger(typeof(IDbManager));
        public static IDbManager dbManager = new MySqlDbManager(Config.database_host, Config.database_username, Config.database_password, Config.database_database);
        public static AI AI = new AI();
        public static Scheduler Scheduler = new Scheduler();
        public static Channel Channel = new Channel();

        static Dictionary<uint, Player> players = new Dictionary<uint, Player>();
        
        static World world = new World();

        static Dictionary<string, SystemVariable> systemVariables = new Dictionary<string, SystemVariable>();

        static bool fireEvents = true;
        public static bool FireEvents {
            get { return fireEvents; }
        }

        static Global() {
        }

        private Global() {
        }

        public static Dictionary<uint, Player> Players {
            get { return players; }
        }

        public static World World {
            get { return world; }
        }

        public static Dictionary<string, SystemVariable> SystemVariables {
            get { return systemVariables; }
        }

        public static void pauseEvents()
        {
            fireEvents = false;
        }

        public static void resumeEvents()
        {
            fireEvents = true;
        }
    }
}
