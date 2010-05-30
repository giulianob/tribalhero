using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Setup;
using Game;
using Game.Battle;
using Game.Fighting;
using log4net.Config;
using log4net;
using System.IO;

namespace ConsoleSimulator {
    class Program {
        static void Main(string[] args) {
            Factory.initAll();
            Global.dbManager.Pause();
            XmlConfigurator.Configure();
            ILog logger = LogManager.GetLogger(typeof(Program));
            
            BattleReport.WriterInit();
            File.Delete("swordsman.csv");
            Simulation sim;
            foreach( KeyValuePair<int,UnitStats> kvp in UnitFactory.dict ) {
                sim = new Simulation((ushort)(kvp.Key/100), kvp.Value.lvl, 1, Simulation.QuantityUnit.GroupSize,false);
                sim.RunDef("swordsman.csv");
                sim = new Simulation((ushort)(kvp.Key / 100), kvp.Value.lvl, 1, Simulation.QuantityUnit.GroupSize, false);
                sim.RunAtk("swordsman.csv");
            }
        }

        static void bm_UnitRemoved(CombatObject co) {
            co.Print();            
        }

        static void bm_ExitTurn(CombatList atk, CombatList def, int turn) {
            System.Console.WriteLine("turn ends");
            System.Console.WriteLine("defenders:");
            foreach (CombatObject co in def) {
                co.Print();
            }
            System.Console.WriteLine("attackers:");
            foreach (CombatObject co in atk) {
                co.Print();
            }           
        }

        static void bm_WithdrawDefender(IEnumerable<CombatObject> list) {
            System.Console.WriteLine("defender leaving:");
            foreach (CombatObject co in list) {
                co.Print();
            }
        }

        static void bm_WithdrawAttacker(IEnumerable<CombatObject> list) {
            System.Console.WriteLine("attacker leaving:");
            foreach (CombatObject co in list) {
                co.Print();
            }
        }

        static void bm_ExitBattle(CombatList atk, CombatList def) {
            System.Console.WriteLine("battle ends");
            System.Console.WriteLine("defenders:");
            foreach (CombatObject co in def) {
                co.Print();
            }
            System.Console.WriteLine("attackers:");
            foreach (CombatObject co in atk) {
                co.Print();
            }
        }
    }
}
