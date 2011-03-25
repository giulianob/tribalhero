#region

using System;
using Game;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;

#endregion

namespace ConsoleSimulator
{
    public class Group
    {
        private static uint player_id;
        private static uint city_id;
        private readonly TroopStub attack;

        private readonly City city;
        private readonly TroopObject obj;
        private readonly Player player;

        public Group()
        {
            player = new Player(player_id, DateTime.MinValue, SystemClock.Now, "player " + player_id, false);
            player_id++;
            var main =
                    new Structure(
                            new StructureStats(new StructureBaseStats("MainBuilding",
                                                                      "",
                                                                      2000,
                                                                      1,
                                                                      0,
                                                                      null,
                                                                      new BaseBattleStats(2000,
                                                                                          1,
                                                                                          WeaponType.Sword,
                                                                                          WeaponClass.Elemental,
                                                                                          ArmorType.Building,
                                                                                          ArmorClass.Stone,
                                                                                          500,
                                                                                          0,
                                                                                          0,
                                                                                          0,
                                                                                          0,
                                                                                          0,
                                                                                          0,
                                                                                          1,
                                                                                          0),
                                                                      0,
                                                                      0,
                                                                      0,
                                                                      ClassId.Structure)));
            city = new City(player, "city " + city_id, Formula.GetInitialCityResources(), Formula.GetInitialCityRadius(), main);
            city_id++;

            attack = new TroopStub();
            attack = new TroopStub();
            attack.AddFormation(FormationType.Normal);
            obj = new TroopObject(attack);
            attack.TroopObject = obj;
            using (new MultiObjectLock(city))
            {
                //attack.City = city;
                city.Troops.Add(attack);
                city.Add(obj);
            }
        }

        public TroopStub Local
        {
            get
            {
                return city.DefaultTroop;
            }
        }

        public TroopStub AttackStub
        {
            get
            {
                return attack;
            }
        }

        public City City
        {
            get
            {
                return city;
            }
        }

        public void AddToLocal(ushort type, byte lvl, ushort count, FormationType formation)
        {
            using (new MultiObjectLock(city))
            {
                city.BeginUpdate();
                city.Template[type] = UnitFactory.GetUnitStats(type, lvl);
                city.EndUpdate();
                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.AddUnit(formation, type, count);
                city.DefaultTroop.EndUpdate();
            }
        }

        public void AddToAttack(ushort type, byte lvl, ushort count, FormationType formation)
        {
            using (new MultiObjectLock(city))
            {
                city.BeginUpdate();
                city.Template[type] = UnitFactory.GetUnitStats(type, lvl);
                city.EndUpdate();
                attack.BeginUpdate();
                attack.AddUnit(formation, type, count);
                attack.EndUpdate();
            }
        }
    }
}