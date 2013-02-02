#region

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Game.Battle;
using Game.Comm;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Database;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Module;
using Game.Module.Remover;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Ninject.Extensions.Logging;
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
        private readonly DbLoader dbLoader;

        private readonly ILogger logger;

        private readonly IPlayerSelectorFactory playerSelector;

        private readonly IPlayersRemoverFactory playersRemoverFactory;

        private readonly IStrongholdManager strongholdManager;

        private readonly IBarbarianTribeManager barbarianTribeManager;

        private readonly IWorld world;

        private readonly SystemVariablesUpdater systemVariablesUpdater;

        private readonly IScheduler scheduler;

        private readonly IDbManager dbManager;

        private readonly StrongholdActivationChecker strongholdActivationChecker;

        private readonly VictoryPointChecker victoryPointChecker;

        private readonly BarbarianTribeChecker barbarianTribeChecker;

        private readonly IPolicyServer policyServer;

        private readonly ITcpServer server;

        private readonly TServer thriftServer;

        public Engine(ILogger logger,
                      ITcpServer server,
                      IPolicyServer policyServer,
                      TServer thriftServer,
                      DbLoader dbLoader,
                      IPlayerSelectorFactory playerSelector,
                      IPlayersRemoverFactory playersRemoverFactory,
                      IStrongholdManager strongholdManager,
                      IBarbarianTribeManager barbarianTribeManager,
                      IWorld world,
                      SystemVariablesUpdater systemVariablesUpdater,
                      IScheduler scheduler,
                      IDbManager dbManager,
                      StrongholdActivationChecker strongholdActivationChecker,
                      VictoryPointChecker victoryPointChecker,
                      BarbarianTribeChecker barbarianTribeChecker)
        {
            this.logger = logger;
            this.server = server;
            this.policyServer = policyServer;
            this.thriftServer = thriftServer;
            this.dbLoader = dbLoader;
            this.playerSelector = playerSelector;
            this.playersRemoverFactory = playersRemoverFactory;
            this.strongholdManager = strongholdManager;
            this.barbarianTribeManager = barbarianTribeManager;
            this.world = world;
            this.systemVariablesUpdater = systemVariablesUpdater;
            this.scheduler = scheduler;
            this.dbManager = dbManager;
            this.strongholdActivationChecker = strongholdActivationChecker;
            this.victoryPointChecker = victoryPointChecker;
            this.barbarianTribeChecker = barbarianTribeChecker;
        }

        public EngineState State { get; private set; }

        public bool Start()
        {
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            }

            logger.Info(@"
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
            {
                throw new Exception("Server is not stopped");
            }

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
                FileStream regionChanges = File.Open(regionChangesPath,
                                                     createRegionChanges ? FileMode.Create : FileMode.Open,
                                                     FileAccess.ReadWrite);

                // Load map
                world.Regions.Load(map,
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
            {
                dbManager.EmptyDatabase();
            }
#endif

            // Load database
            if (!dbLoader.LoadFromDatabase())
            {
                logger.Error("Failed to load database");
                return false;
            }

            // Initialize stronghold
            if (Config.stronghold_generate > 0 && strongholdManager.Count == 0) // Only generate if there is none.
            {
                strongholdManager.Generate(Config.stronghold_generate);
            }            
            strongholdActivationChecker.Start(TimeSpan.FromSeconds(Config.stronghold_activation_check_interval_in_sec));            
            victoryPointChecker.Start();

            // Initialize barbarian tribes
            if (Config.barbariantribe_generate > 0 && barbarianTribeManager.Count < Config.barbariantribe_generate) // Only generate if there is none.
            {
                barbarianTribeManager.Generate(Config.barbariantribe_generate - barbarianTribeManager.Count);
            }            
            barbarianTribeChecker.Start(TimeSpan.FromSeconds(Config.barbariantribe_idle_check_interval_in_sec));

            // Initialize game market
            Market.Init();

            // Create NPC if specified
            if (Config.ai_enabled)
            {
                Ai.Init();
            }

            // Start command processor
            server.Start();

            // Start policy server
            policyServer.Start();

            // Start thrift server
            ThreadPool.QueueUserWorkItem(o => thriftServer.Serve());

            // Schedule player deletions
            if (Config.players_remove_idle)
            {
                ThreadPool.QueueUserWorkItem(
                                             o =>
                                             playersRemoverFactory.CreatePlayersRemover(
                                                                                        playerSelector
                                                                                                .CreateNewbieIdleSelector
                                                                                                ()).Start());
            }

            State = EngineState.Started;

            return true;
        }

        public static void CreateDefaultKernel()
        {
            Ioc.Kernel = new StandardKernel(new NinjectSettings {LoadExtensions = true}, new GameModule());

            // Instantiate singletons here for now until all classes are properly being injected
            SystemVariablesUpdater.Current = Ioc.Kernel.Get<SystemVariablesUpdater>();
            RadiusLocator.Current = Ioc.Kernel.Get<RadiusLocator>();
            TileLocator.Current = Ioc.Kernel.Get<TileLocator>();
            ReverseTileLocator.Current = Ioc.Kernel.Get<ReverseTileLocator>();
            BattleFormulas.Current = Ioc.Kernel.Get<BattleFormulas>();
            Concurrency.Current = Ioc.Kernel.Get<ILocker>();
            Formula.Current = Ioc.Kernel.Get<Formula>();
            World.Current = Ioc.Kernel.Get<IWorld>();
            Procedure.Current = Ioc.Kernel.Get<Procedure>();
            Scheduler.Current = Ioc.Kernel.Get<IScheduler>();
            DbPersistance.Current = Ioc.Kernel.Get<IDbManager>();
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            logger.Error(ex, "Unhandled Exception");
        }

        public void Stop()
        {
            if (State != EngineState.Started)
            {
                throw new Exception("Server is not started");
            }

            State = EngineState.Stopping;

            systemVariablesUpdater.Pause();
            logger.Info("Stopping TCP server...");
            server.Stop();
            logger.Info("Stopping policy server...");
            policyServer.Stop();
            logger.Info("Waiting for scheduler to end...");
            //thriftServer.Stop();
            scheduler.Pause();
            world.Regions.Unload();
            Global.Logger.Info("Goodbye!");

            State = EngineState.Stopped;
        }
    }
}