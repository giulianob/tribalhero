#region

using System;
using System.IO;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Logic;
using Game.Module;
using Game.Setup;
using log4net;

#endregion

namespace Game
{
    public enum EngineState
    {
        Stopped,
        Stopping,
        Started,
        Starting
    }

    public class Engine
    {
        private static TcpServer server;

        private static PolicyServer policyServer;

        public static EngineState State { get; private set; }

        public static bool Start()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            ILog logger = LogManager.GetLogger(typeof(Engine));
            logger.Info(
                        @"
_________ _______ _________ ______   _______  _       
\__   __/(  ____ )\__   __/(  ___ \ (  ___  )( \      
   ) (   | (    )|   ) (   | (   ) )| (   ) || (      
   | |   | (____)|   | |   | (__/ / | (___) || |      
   | |   |     __)   | |   |  __ (  |  ___  || |      
   | |   | (\ (      | |   | (  \ \ | (   ) || |      
   | |   | ) \ \_____) (___| )___) )| )   ( || (____/\
   )_(   |/   \__/\_______/|/ \___/ |/     \|(_______/
                                                      
          _______  _______  _______ 
|\     /|(  ____ \(  ____ )(  ___  )
| )   ( || (    \/| (    )|| (   ) |
| (___) || (__    | (____)|| |   | |
|  ___  ||  __)   |     __)| |   | |
| (   ) || (      | (\ (   | |   | |
| )   ( || (____/\| ) \ \__| (___) |
|/     \|(_______/|/   \__/(_______)");

            if (State != EngineState.Stopped)
                throw new Exception("Server is not stopped");

            State = EngineState.Starting;

            // Initialize all of the factories
            Factory.InitAll();

            // Load map
            using (var map = new FileStream(Config.maps_folder + "map.dat", FileMode.Open))
            {
                // Create region changes file or open it depending on config settings
                string regionChangesPath = Config.regions_folder + "region_changes.dat";
#if DEBUG
                bool createRegionChanges = Config.database_empty || !File.Exists(regionChangesPath);
#else
                bool createRegionChanges = !File.Exists(regionChangesPath);
#endif
                FileStream regionChanges = File.Open(regionChangesPath, createRegionChanges ? FileMode.Create : FileMode.Open, FileAccess.ReadWrite);

                // Load map
                Global.World.Load(map,
                                  regionChanges,
                                  createRegionChanges,
                                  Config.map_width,
                                  Config.map_height,
                                  Config.region_width,
                                  Config.region_height,
                                  Config.city_region_width,
                                  Config.city_region_height);
            }

            // Empty database if specified
#if DEBUG
            if (Config.database_empty)
                Global.DbManager.EmptyDatabase();
#endif

            // Load database
            if (!DbLoader.LoadFromDatabase(Global.DbManager))
                return false;

            // Initialize game market
            Market.Init();

            // Create NPC if specified
            if (Config.ai_enabled)
                Ai.Init();

            // Initialize command processor
            var processor = new Processor();

            // Start accepting connections
            server = new TcpServer(processor);
            server.Start();

            // Initialize policy server
            policyServer = new PolicyServer();
            policyServer.Start();

            State = EngineState.Started;

            return true;
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Global.Logger.Error("Unhandled Exception", ex);
        }

        public static void Stop()
        {
            if (State != EngineState.Started)
                throw new Exception("Server is not started");

            State = EngineState.Stopping;

            SystemVariablesUpdater.Pause();
            Global.Logger.Info("Stopping TCP server...");
            server.Stop();
            Global.Logger.Info("Stopping policy server...");
            policyServer.Stop();
            Global.Logger.Info("Waiting for scheduler to end...");
            Global.Scheduler.Pause();
            Global.World.Unload();
            Global.Logger.Info("Goodbye!");

            State = EngineState.Stopped;
        }
    }
}