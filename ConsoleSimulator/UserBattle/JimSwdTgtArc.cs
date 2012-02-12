using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;

namespace ConsoleSimulator.UserBattle {
    public class JimSwdTgtArc {
        public JimSwdTgtArc() {
    /*        Global.FireEvents = false;
            Factory.CompileConfigFiles();
            Factory.InitAll();
            Global.DbManager.Pause();*/
        }

        public void TestCleanup() {
        }

        public void Run() {
            Group defender = new Group();
            defender.AddToLocal(UnitType.Fighter, 3, 25);
            defender.AddToLocal(UnitType.Bowman, 3, 40);
            defender.AddToLocal(UnitType.Swordsman, 3, 95);
            defender.AddToLocal(UnitType.Archer, 3, 100);
            defender.AddStructure(StructureType.Tower, 4);
            defender.AddStructure(StructureType.TradingPost, 5);
            int defUpkeep = defender.Upkeep();

            Group attacker = new Group();
            attacker.AddToAttack(UnitType.Swordsman, 6, 82);
            int atkUpkeep = attacker.Upkeep();

            Simulation sim = new Simulation(attacker, defender);
            sim.RunTill(6);
            
        }

    }
}
