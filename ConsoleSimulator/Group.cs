#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace ConsoleSimulator
{
    public enum StructureType
    {
        Barrack = 2204,
        Farm = 2106,
        Tower = 2402,
        TradingPost = 2501,
    }

    public enum UnitType {
        Fighter = 11,
        Bowman = 12,

        Swordsman = 101,
        Archer = 102,
        Pikeman = 103,
        Gladiator = 104,

        Cavalry = 105,
        Knight = 106,

        Helepolis = 107,
        Catapult = 108,

        TestSwordsman =1001,
    }

    public class Group
    {
        private static uint playerId;
        private static uint cityId;
        private readonly TroopStub attack;
        private List<Structure> structures = new List<Structure>();

        private readonly ICity city;
        private readonly TroopObject obj;
        private readonly Player player;

        public Group()
        {
            player = new Player(playerId, DateTime.MinValue, SystemClock.Now, "player " + playerId, string.Empty, false);
            playerId++;
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
                                                                                          ArmorType.Building3,
                                                                                          ArmorClass.Stone,
                                                                                          500,
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
            city = new City(player, "city " + cityId, Formula.Current.GetInitialCityResources(), Formula.Current.GetInitialCityRadius(), main);
            player.Add(city);
            cityId++;

            attack = new TroopStub();
            attack = new TroopStub();
            attack.AddFormation(FormationType.Normal);
            obj = new TroopObject(attack);
            attack.TroopObject = obj;
            using (Concurrency.Current.Lock(city))
            {
                //attack.City = city;
                city.Troops.Add(attack);
                city.Add(obj);
            }
        }

        public ITroopStub Local
        {
            get
            {
                return city.DefaultTroop;
            }
        }

        public ITroopStub AttackStub
        {
            get
            {
                return attack;
            }
        }

        public ICity City
        {
            get
            {
                return city;
            }
        }

        public List<Structure> Structures
        {
            get
            {
                return structures;
            }
        }

        public void AddToLocal(UnitType type, byte lvl, ushort count)
        {
            AddToLocal((ushort)type, lvl, count, FormationType.Normal);
        }

        public void AddToLocal(ushort type, byte lvl, ushort count, FormationType formation)
        {
            using (Concurrency.Current.Lock(city))
            {
                city.BeginUpdate();
                city.Template[type] = Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type, lvl);
                if(city.Template[type]==null) throw  new Exception("Unit type not found!");
                city.EndUpdate();
                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.AddUnit(formation, type, count);
                city.DefaultTroop.EndUpdate();
            }
        }
        public void AddStructure(StructureType type, byte lvl)
        {
            AddStructure((ushort)type,lvl);
        }
        public void AddStructure(ushort type, byte lvl)
        {
            Structure structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(type, lvl);
            city.Add(structure);
            structures.Add(structure);
        }

        public void AddToAttack(UnitType type, byte lvl, ushort count)
        {
            AddToAttack((ushort)type,lvl,count, FormationType.Normal);
        }

        public void AddToAttack(ushort type, byte lvl, ushort count, FormationType formation)
        {
            using (Concurrency.Current.Lock(city))
            {
                city.BeginUpdate();
                city.Template[type] = Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type, lvl);
                if (city.Template[type] == null) throw new Exception("Unit type not found!");
                city.EndUpdate();
                attack.BeginUpdate();
                attack.AddUnit(formation, type, count);
                attack.EndUpdate();
            }
        }

        public int Upkeep(UnitType type)
        {

            return attack[FormationType.Normal][(ushort)type] * city.Template[(ushort)type].Upkeep;
        }

        public int Upkeep()
        {
            return city.DefaultTroop.Upkeep;
        }
    }
}