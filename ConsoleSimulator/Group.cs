using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using Game.Data;
using Game.Fighting;
using Game.Setup;

namespace ConsoleSimulator {
    public class Group {
        static uint player_id = 0;
        static uint city_id = 0;

        Player player;
        City city;
        TroopObject obj;
        TroopStub attack;

        public Group() {
            player = new Player(player_id, "player " + player_id);
            player_id++;
            Structure main = new Structure(2000,1,new StructureStats(new BattleStats(),0));
            city = new City(player, "city " + city_id, new Resource(), main);
            city_id++;

            attack = new TroopStub(city.Troops);
            attack.addFormation(FormationType.Normal);
            obj = new TroopObject(attack);
            attack.TroopObject = obj;
            attack.City = city;
            city.Troops.Add(attack);
            city.add(obj);
        }

        public void AddToLocal(ushort type, byte lvl, ushort count, FormationType formation) {
            city.Template[type] = UnitFactory.getUnitStats(type, lvl);
            city.DefaultTroop.addUnit(formation, type, count);
        }

        public void AddToAttack(ushort type, byte lvl, ushort count, FormationType formation) {
            city.Template[type] = UnitFactory.getUnitStats(type,lvl);
            attack.addUnit(formation, type, count);
        }

        public TroopStub Local {
            get { return city.DefaultTroop; }
        }

        public TroopStub AttackStub {
            get { return attack; }
        }
        public City City {
            get { return city; }
        }
    }
}
