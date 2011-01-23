using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using Game.Data;
using Game.Fighting;
using Game.Setup;
using Game.Data.Troop;
using Game.Data.Stats;
using Game.Util;

namespace ConsoleSimulator {
    public class Group {
        static uint player_id = 0;
        static uint city_id = 0;

        Player player;
        City city;
        TroopObject obj;
        TroopStub attack;

        public Group() {
            player = new Player(player_id, DateTime.MinValue, SystemClock.Now, "player " + player_id, false, false);
            player_id++;
            Structure main = new Structure(new StructureStats(new StructureBaseStats("MainBuilding", "", 2000, 1, 0, null, new BaseBattleStats(2000, 1, WeaponType.SWORD, WeaponClass.ELEMENTAL, ArmorType.BUILDING, ArmorClass.STONE, 500, 0, 0, 0, 0, 0, 0, 1, 0), 0, 0, 0, ClassId.STRUCTURE)));
            city = new City(player, "city " + city_id, new Resource(), main);
            city_id++;

            attack = new TroopStub();
            attack = new TroopStub();
            attack.AddFormation(FormationType.NORMAL);
            obj = new TroopObject(attack);
            attack.TroopObject = obj;
            using (new MultiObjectLock(city)) {
                //attack.City = city;
                city.Troops.Add(attack);
                city.Add(obj);
            }
        }

        public void AddToLocal(ushort type, byte lvl, ushort count, FormationType formation) {
            using (new MultiObjectLock(city)) {
                city.BeginUpdate();
                city.Template[type] = UnitFactory.GetUnitStats(type, lvl);
                city.EndUpdate();
                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.AddUnit(formation, type, count);
                city.DefaultTroop.EndUpdate();
            }
        }

        public void AddToAttack(ushort type, byte lvl, ushort count, FormationType formation) {
            using (new MultiObjectLock(city)) {
                city.BeginUpdate();
                city.Template[type] = UnitFactory.GetUnitStats(type, lvl);
                city.EndUpdate();
                attack.BeginUpdate();
                attack.AddUnit(formation, type, count);
                attack.EndUpdate();
            }
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
