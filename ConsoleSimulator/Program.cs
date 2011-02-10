#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Setup;
using log4net;
using log4net.Config;

#endregion

namespace ConsoleSimulator
{
    class Program
    {
        private static void Main(string[] args)
        {
            bool sameLevelOnly = true;
            var lvlFilter = new List<byte> {1, 10};

            Factory.CompileConfigFiles();
            // CSVToXML.Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);
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
                FileStream regionChanges = File.Open(regionChangesPath,
                                                     createRegionChanges ? FileMode.Create : FileMode.Open,
                                                     FileAccess.ReadWrite);

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

            Global.DbManager.Pause();
            XmlConfigurator.Configure();
            ILog logger = LogManager.GetLogger(typeof(Program));

            //      BattleReport.WriterInit();

            Array.ForEach(Directory.GetFiles(Directory.GetCurrentDirectory(), "Simulation*.csv"),
                          delegate(string path) { File.Delete(path); });

            Simulation sim;
            foreach (var kvp in UnitFactory.GetList())
            {
                if (!lvlFilter.Any(x => x == kvp.Value.Lvl))
                    continue;
                sim = new Simulation((ushort)(kvp.Key/100),
                                     kvp.Value.Lvl,
                                     1,
                                     Simulation.QuantityUnit.GroupSize,
                                     sameLevelOnly);
                sim.RunDef("Simulation " + kvp.Value.Lvl + ".csv");
                sim = new Simulation((ushort)(kvp.Key/100),
                                     kvp.Value.Lvl,
                                     1,
                                     Simulation.QuantityUnit.GroupSize,
                                     sameLevelOnly);
                sim.RunAtk("Simulation " + kvp.Value.Lvl + ".csv");
            }
        }

        private static void bm_UnitRemoved(CombatObject co)
        {
            co.Print();
        }

        private static void bm_ExitTurn(CombatList atk, CombatList def, int turn)
        {
            Console.WriteLine("turn ends");
            Console.WriteLine("defenders:");
            foreach (var co in def)
                co.Print();
            Console.WriteLine("attackers:");
            foreach (var co in atk)
                co.Print();
        }

        private static void bm_WithdrawDefender(IEnumerable<CombatObject> list)
        {
            Console.WriteLine("defender leaving:");
            foreach (var co in list)
                co.Print();
        }

        private static void bm_WithdrawAttacker(IEnumerable<CombatObject> list)
        {
            Console.WriteLine("attacker leaving:");
            foreach (var co in list)
                co.Print();
        }

        private static void bm_ExitBattle(CombatList atk, CombatList def)
        {
            Console.WriteLine("battle ends");
            Console.WriteLine("defenders:");
            foreach (var co in def)
                co.Print();
            Console.WriteLine("attackers:");
            foreach (var co in atk)
                co.Print();
        }
    }
}