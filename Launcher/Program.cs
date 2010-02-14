#region

using System.IO;
using System.Threading;
using Game.Battle;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Module;
using Game.Setup;
using log4net.Config;

#endregion

namespace Launcher {
    class Program {
        private static void Main(string[] args) {
            ThreadPool.SetMaxThreads(20, 20);            
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
            if (Config.database_empty)
                Global.dbManager.EmptyDatabase();

            // Load database
            if (!DbLoader.LoadFromDatabase(Global.dbManager))
                return;

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
            TcpServer server = new TcpServer(processor);
            server.Start();

            while (true)
                Thread.Sleep(2000);
        }
    }
}