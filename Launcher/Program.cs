#region

using System.IO;
using System.Threading;
using Game.Battle;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Module;
using Game.Setup;
using log4net;
using log4net.Config;

#endregion

namespace Game {
    class Program {
        private static void Main(string[] args) {
            ThreadPool.SetMaxThreads(20, 20);
            XmlConfigurator.Configure();
            ILog logger = LogManager.GetLogger(typeof (Program));
            Factory.initAll();
            CSVToXML.Program.Main(null);

            using (FileStream map = new FileStream(Config.maps_folder + "map.dat", FileMode.Open)) {
                Global.World.Load(map, Config.map_width, Config.map_height, Config.region_width, Config.region_height,
                                  Config.city_region_width, Config.city_region_height);
            }
            Player resourcePlayer = new Player(0, "Player 0");
            City resourceCity = new City(resourcePlayer, "Resource City", new Resource(), null);

            if (Config.database_empty)
                Global.dbManager.EmptyDatabase();

            if (!DbLoader.LoadFromDatabase(Global.dbManager))
                return;

            BattleReport.WriterInit();
            Market.Init();

            if (Config.ai_enabled)
                AI.Init();

            Processor processor = new Processor();
            TcpServer server = new TcpServer(processor);
            server.start();

            while (true)
                Thread.Sleep(2000);
        }
    }
}