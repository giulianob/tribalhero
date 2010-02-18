#region

using System;
using System.IO;
using System.Threading;
using Game.Battle;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Logic;
using Game.Module;
using Game.Setup;
using log4net.Config;

#endregion

namespace Launcher {
    public class GameLauncher {
        static TcpServer server;

        public static bool Start() {
            XmlConfigurator.Configure();

            // Initialize all of the factories
            Factory.InitAll();

            // Convert the CSV to XML for the client
            CSVToXML.Program.Main(null);

            // Load map
            using (FileStream map = new FileStream(Config.maps_folder + "map.dat", FileMode.Open)) {
                Global.World.Load(map, Config.map_width, Config.map_height, Config.region_width, Config.region_height,
                                  Config.city_region_width, Config.city_region_height);
            }

            // Empty database if specified
#if DEBUG
            if (Config.database_empty) {
                Global.dbManager.EmptyDatabase();
            }
#endif

            // Load database
            if (!DbLoader.LoadFromDatabase(Global.dbManager))
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

            return true;
        }

        public static void Stop() {
            SystemTimeUpdater.Pause();
            Global.Logger.Info("Stopping TCP Server...");
            server.Stop();
            Global.Logger.Info("Waiting for scheduler to end...");
            Global.Scheduler.Pause();
            Global.Logger.Info("Goodbye!");
        }
    }

    public class Program {
        public static void Main(string[] args) {
            GameLauncher.Start();

            while (true) {
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key != ConsoleKey.Q || key.Modifiers != ConsoleModifiers.Alt)
                    continue;

                GameLauncher.Stop();
                Environment.Exit(0);
            }
        }
    }
}