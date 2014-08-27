#region

using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Game.Comm;
using Game.Comm.Channel;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Database;
using Game.Logic;
using Game.Map;
using Game.Module;
using Game.Module.Remover;
using Game.Setup;
using Game.Setup.DependencyInjection;
using Game.Util;
using Mono.Unix;
using Persistance;
using SimpleInjector;
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

        private static readonly ILogger Logger = LoggerFactory.Current.GetLogger<Engine>();

        private readonly IPlayerSelectorFactory playerSelector;

        private readonly IPlayersRemoverFactory playersRemoverFactory;

        private readonly IStrongholdManager strongholdManager;

        private readonly IBarbarianTribeManager barbarianTribeManager;

        private readonly IWorld world;

        private readonly SystemVariablesUpdater systemVariablesUpdater;

        private readonly IScheduler scheduler;

        private readonly IDbManager dbManager;

        private readonly StrongholdActivationChecker strongholdActivationChecker;

        private readonly StrongholdChecker strongholdChecker;

        private readonly BarbarianTribeChecker barbarianTribeChecker;

        private readonly ICityChannel cityChannel;

        private readonly IStrongholdManagerLogger strongholdManagerLogger;

        private readonly StoreSync storeSync;

        private readonly IQueueListener queueListener;

        private readonly IPolicyServer policyServer;

        private readonly INetworkServer server;

        private readonly TServer thriftServer;

        public Engine(INetworkServer server,
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
                      StrongholdChecker strongholdChecker,
                      BarbarianTribeChecker barbarianTribeChecker,
                      ICityChannel cityChannel,
                      IStrongholdManagerLogger strongholdManagerLogger,
                      StoreSync storeSync,
                      IQueueListener queueListener)
        {
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
            this.strongholdChecker = strongholdChecker;
            this.barbarianTribeChecker = barbarianTribeChecker;
            this.cityChannel = cityChannel;
            this.strongholdManagerLogger = strongholdManagerLogger;
            this.storeSync = storeSync;
            this.queueListener = queueListener;
        }

        public EngineState State { get; private set; }

        public static void AttachExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Logger.Error("Unhandled exception in app domain. Sender: ", sender);

                UnhandledExceptionHandler(e.ExceptionObject);
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Logger.Error("Unhandled exception in task. Sender: ", sender);

                UnhandledExceptionHandler(e.Exception);
            };
        }

        public static void UnhandledExceptionHandler(object ex)
        {
            try
            {
                if (ex == null)
                {
                    var stackTrace = Environment.StackTrace;
                    Logger.Error("Unhandled exception but did not have exception object. Stack trace:", stackTrace);
                    return;
                }

                if (!(ex is Exception))
                {
                    Logger.Error("Unhandled exception. Object was not exception. {0}", ex);
                    return;
                }

                var aggregateException = ex as AggregateException;
                if (aggregateException != null)
                {                    
                    Logger.ErrorException("Unhandled exception aggregate", aggregateException);
                    foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                    {
                        Logger.ErrorException("Aggregate inner exception", innerException);
                    }
                    return;
                }

                Logger.ErrorException("Unhandled exception", (Exception)ex);
            }
            finally
            {
                if (Global.IsRunningOnMono())
                {
                    UnixProcess.GetCurrentProcess().Kill();
                }
                else
                {
                    Environment.FailFast("Unhandled exception.. failing fast.");
                }
            }
        }

        public void Start()
        {
            if (Global.IsRunningOnMono())
            {
                ThreadPool.SetMaxThreads(200, 50);
                ThreadPool.SetMinThreads(100, 50);
            }

            Logger.Info(@"
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
            using (var mapFile = new FileStream(Config.maps_folder + "map.dat", FileMode.Open))
            using (var map = new GZipStream(mapFile, CompressionMode.Decompress))
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
                                           Config.minimap_region_width,
                                           Config.minimap_region_height);
            }

#if DEBUG
            if (Config.server_production)
            {
                throw new Exception("Trying to run debug on production server");                
            }
#endif

            // Empty database if specified
#if DEBUG
            if (Config.database_empty)
            {
                dbManager.EmptyDatabase();
            }
#endif

            // Initiate city channel
            cityChannel.Register(world.Cities);

            // Load database
            dbLoader.LoadFromDatabase();

            // Initialize stronghold
            if (Config.stronghold_generate > 0 && strongholdManager.Count == 0) // Only generate if there is none.
            {
                strongholdManager.Generate(Config.stronghold_generate);
            }            

            strongholdActivationChecker.Start(TimeSpan.FromSeconds(Config.stronghold_activation_check_interval_in_sec));            
            strongholdChecker.Start();
            strongholdManagerLogger.Listen(strongholdManager);

            // Initialize barbarian tribes
            if (Config.barbariantribe_generate > 0 && barbarianTribeManager.Count < Config.barbariantribe_generate) // Only generate if there is none.
            {
                barbarianTribeManager.Generate(Config.barbariantribe_generate - barbarianTribeManager.Count);
            }            
            barbarianTribeChecker.Start(TimeSpan.FromSeconds(Config.barbariantribe_idle_check_interval_in_sec));

            // Initialize game market
            Market.Init();
            
            // Start listening  for queue events
            if (!Config.queue_disabled)
            {
                queueListener.Start(Config.api_domain);
            }

            // Start store sync
            storeSync.Start();

            // Start command processor
            server.Start(Config.server_listen_address, Config.server_port);

            // Start policy server
            policyServer.Start(Config.server_listen_address, 843);

            // Start thrift server
            ThreadPool.QueueUserWorkItem(o => thriftServer.Serve());

            // Schedule player deletions
            if (Config.players_remove_idle)
            {
                playersRemoverFactory.CreatePlayersRemover(playerSelector.CreateNewbieIdleSelector()).Start();
            }

            State = EngineState.Started;
        }

        public static IKernel CreateDefaultKernel(params IKernelModule[] extraModules)
        {
            var container = new Container();
            new GameModule().Load(container);
            foreach (var module in extraModules)
            {
                module.Load(container);
            }
            container.Verify();

            // Instantiate singletons here for now until all classes are properly being injected
            Ioc.Kernel = new Ioc(container);

            PacketHelper.RegionLocator = Ioc.Kernel.Get<IRegionLocator>();
            Global.Current = Ioc.Kernel.Get<Global>();
            SystemVariablesUpdater.Current = Ioc.Kernel.Get<SystemVariablesUpdater>();
            World.Current = Ioc.Kernel.Get<IWorld>();
            Scheduler.Current = Ioc.Kernel.Get<IScheduler>();
            DbPersistance.Current = Ioc.Kernel.Get<IDbManager>();

            return Ioc.Kernel;
        }

        public void Stop()
        {
            if (State != EngineState.Started)
            {
                throw new Exception("Server is not started");
            }

            State = EngineState.Stopping;

            Logger.Info("Stopping queue listener...");
            queueListener.Stop();

            Logger.Info("Pausing system variable updates");
            systemVariablesUpdater.Pause();
            
            Logger.Info("Stopping TCP server...");
            server.Stop();

            Logger.Info("Stopping policy server...");
            policyServer.Stop();

            Logger.Info("Stopping thrift server");
            thriftServer.Stop();

            Logger.Info("Waiting for scheduler to end...");
            scheduler.Pause();

            Logger.Info("Closing regions file...");
            world.Regions.Unload();
            
            Logger.Info("Goodbye!");

            State = EngineState.Stopped;
        }
    }
}