using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using Game.Setup;
using Game.Comm;
using Game.Util;
using Game.Data;
using Game.Logic;
using Game.Logic.Actions;
using log4net;
using log4net.Config;
using Game.Database;
using Game.Module;
using Game.Battle;

namespace Game {
    class Program {

        static void Main(string[] args) {
            ThreadPool.SetMaxThreads(20, 20);
            XmlConfigurator.Configure();
            ILog logger = LogManager.GetLogger(typeof(Program));            
            Factory.initAll();
            CSVToXML.Program.Main(null);
            Global.World.load(new FileStream("map.dat", FileMode.Open), Config.map_width, Config.map_height, Config.region_width, Config.region_height, Config.city_region_width, Config.city_region_height);
            Player resourcePlayer = new Player(0, "Player 0");
            City resourceCity = new City(resourcePlayer, "Resource City", new Resource(), null);

            if (Setup.Config.database_empty) 
                Global.dbManager.EmptyDatabase();

            if (!Database.DbLoader.LoadFromDatabase(Global.dbManager))
                return;

            BattleReport.WriterInit();
            Market.Init();

            if (Config.ai_enabled)
                AI.Init();

            Processor processor = new Processor();
            TcpServer server = new TcpServer(processor);
            server.start();

            while (true) {
                Thread.Sleep(2000);

            }
        }

    }
}