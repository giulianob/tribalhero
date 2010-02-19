#region

using System;
using System.IO;
using Game.Battle;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Logic;
using Game.Module;
using Game.Setup;
using log4net.Config;

#endregion

namespace Game {            
    public enum EngineState {
            STOPPED,
            STOPPING,
            STARTED, 
            STARTING
        }

    public class Engine {
        static TcpServer server;
        public static EngineState State { get; private set; }

        public static bool Start() {
            if (State != EngineState.STOPPED)
                throw new Exception("Server is not stopped");

            State = EngineState.STARTING;

            XmlConfigurator.Configure();

            // Initialize all of the factories
            Factory.InitAll();

            // Load map
            using (FileStream map = new FileStream(Config.maps_folder + "map.dat", FileMode.Open)) {
                Global.World.Load(map, Config.map_width, Config.map_height, Config.region_width, Config.region_height,
                                  Config.city_region_width, Config.city_region_height);
            }

            // Empty database if specified
#if DEBUG
            if (Config.database_empty) {
                Global.DbManager.EmptyDatabase();
            }
#endif

            // Load database
            if (!DbLoader.LoadFromDatabase(Global.DbManager))
                return false;

            // Initialize battle report loggers
            BattleReport.WriterInit();

            // Initialize game market
            Market.Init();

            // Create NPC if specified
            if (Config.ai_enabled)
                AI.Init();

            // Initialize command processor
            Processor processor = new Processor();

            // Start accepting connections
            server = new TcpServer(processor);
            server.Start();

            State = EngineState.STARTED;

            return true;
        }

        public static void Stop() {
            if (State != EngineState.STARTED)
                throw new Exception("Server is not started");

            State = EngineState.STOPPING;

            SystemTimeUpdater.Pause();
            Global.Logger.Info("Stopping TCP Server...");
            server.Stop();
            Global.Logger.Info("Waiting for scheduler to end...");
            Global.Scheduler.Pause();
            Global.Logger.Info("Goodbye!");

            State = EngineState.STOPPED;
        }
    }
}