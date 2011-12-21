#region

using System;
using System.IO;
using Game.Battle;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;
using Ninject.Extensions.Logging;
using Ninject.Extensions.Logging.Log4net;
using Persistance;
using Thrift.Server;

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
        private readonly ILogger logger;
        private readonly IPolicyServer policyServer;
        private readonly TServer thriftServer;
        private readonly ITcpServer server;        

        public EngineState State { get; private set; }

        public Engine(ILogger logger, ITcpServer server, IPolicyServer policyServer, TServer thriftServer)
        {
            this.logger = logger;
            this.server = server;
            this.policyServer = policyServer;
            this.thriftServer = thriftServer;
        }

        public bool Start()
        {            
            if (!System.Diagnostics.Debugger.IsAttached)
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

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
                World.Current.Load(map,
                                  regionChanges,
                                  createRegionChanges,
                                  Config.map_width,
                                  Config.map_height,
                                  Config.region_width,
                                  Config.region_height,
                                  Config.city_region_width,
                                  Config.city_region_height);
            }

#if DEBUG
            if (Config.server_production)
            {
                logger.Error("Trying to run debug on production server");
                return false;
            }
#endif

            // Empty database if specified
#if DEBUG
            if (Config.database_empty)
                Ioc.Kernel.Get<IDbManager>().EmptyDatabase();
#endif

            // Load database
            if (!DbLoader.LoadFromDatabase(Ioc.Kernel.Get<IDbManager>()))
            {
                logger.Error("Failed to load database");
                return false;
            }

            // Initialize game market
            Market.Init();

            // Create NPC if specified
            if (Config.ai_enabled)
                Ai.Init();

            // Start command processor
            server.Start();

            // Start policy server
            policyServer.Start();

            // Start thrift server
            thriftServer.Serve();

            State = EngineState.Started;

            return true;
        }

        public static IKernel CreateDefaultKernel()
        {
            Ioc.Kernel = new StandardKernel(new NinjectSettings { LoadExtensions = true }, new GameModule());
            
            // Instantiate singletons here for now
            RadiusLocator.Current = Ioc.Kernel.Get<RadiusLocator>();
            BattleFormulas.Current = Ioc.Kernel.Get<BattleFormulas>();
            Concurrency.Current = Ioc.Kernel.Get<ILocker>();
            Formula.Current = Ioc.Kernel.Get<Formula>();
            World.Current = Ioc.Kernel.Get<World>();
            Procedure.Current = Ioc.Kernel.Get<Procedure>();            

            return Ioc.Kernel;
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            logger.Error(ex, "Unhandled Exception");
        }

        public void Stop()
        {
            if (State != EngineState.Started)
                throw new Exception("Server is not started");

            State = EngineState.Stopping;

            SystemVariablesUpdater.Pause();
            logger.Info("Stopping TCP server...");
            server.Stop();
            logger.Info("Stopping policy server...");
            policyServer.Stop();
            logger.Info("Waiting for scheduler to end...");
            Scheduler.Current.Pause();
            World.Current.Unload();
            Global.Logger.Info("Goodbye!");

            State = EngineState.Stopped;
        }
    }
}