#region

using System;
using System.Collections.Generic;
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

    public enum UnitType
    {
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

        TestSwordsman = 1001,
    }

    public class Group
    {
        private static uint playerId;

        private static uint cityId;

        private readonly ICity city;

        private readonly TroopObject obj;

        private readonly IPlayer player;

        private readonly List<IStructure> structures = new List<IStructure>();

        public Group()
        {
            player = new Player(playerId,
                                DateTime.MinValue,
                                SystemClock.Now,
                                "player " + playerId,
                                string.Empty,
                                PlayerRights.Basic);
            playerId++;
            BaseBattleStats baseBattleStats = new BaseBattleStats(type: 2000,
                                                                  lvl: 1,
                                                                  weapon: WeaponType.Sword,
                                                                  wpnClass: WeaponClass.Elemental,
                                                                  armor: ArmorType.Building3,
                                                                  armrClass: ArmorClass.Stone,
                                                                  maxHp: 500,
                                                                  atk: 0,
                                                                  splash: 0,
                                                                  range: 0,
                                                                  stealth: 0,
                                                                  speed: 0,
                                                                  groupSize: 1,
                                                                  carry: 0,
                                                                  normalizedCost: 0);
            StructureBaseStats structureBaseStats = new StructureBaseStats(name: "MainBuilding",
                                                                           spriteClass: "",
                                                                           type: 2000,
                                                                           lvl: 1,
                                                                           radius: 0,
                                                                           cost: null,
                                                                           baseBattleStats: baseBattleStats,
                                                                           maxLabor: 0,
                                                                           buildTime: 0,
                                                                           workerId: 0);
            StructureStats structurestats = new StructureStats(structureBaseStats);

            var main = new Structure(structurestats);

            city = new City(id: cityId,
                            owner: player,
                            name: "city " + cityId,
                            resource: Formula.Current.GetInitialCityResources(),
                            radius: Formula.Current.GetInitialCityRadius(),
                            mainBuilding: main,
                            ap: 0);
            player.Add(city);
            cityId++;

            AttackStub = new TroopStub(0, city);
            AttackStub.AddFormation(FormationType.Normal);
            obj = new TroopObject(AttackStub);
            using (Concurrency.Current.Lock(city))
            {
                city.Troops.Add(AttackStub);
                city.Add(obj);
            }

            TroopObject = new TroopObject(AttackStub) {X = city.X, Y = city.Y};
            TroopObject.BeginUpdate();
            TroopObject.Stats = new TroopStats(Formula.Current.GetTroopRadius(AttackStub, null),
                                               Formula.Current.GetTroopSpeed(AttackStub));
            TroopObject.EndUpdate();

            city.Add(TroopObject);
        }

        public ITroopStub Local
        {
            get
            {
                return city.DefaultTroop;
            }
        }

        public ITroopObject TroopObject { get; set; }

        public TroopStub AttackStub { get; private set; }

        public ICity City
        {
            get
            {
                return city;
            }
        }

        public List<IStructure> Structures
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
                if (city.Template[type] == null)
                {
                    throw new Exception("Unit type not found!");
                }
                city.EndUpdate();
                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.AddUnit(formation, type, count);
                city.DefaultTroop.EndUpdate();
            }
        }

        public void AddStructure(StructureType type, byte lvl)
        {
            AddStructure((ushort)type, lvl);
        }

        public void AddStructure(ushort type, byte lvl)
        {
            IStructure structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(type, lvl);
            city.Add(structure);
            structures.Add(structure);
        }

        public void AddToAttack(UnitType type, byte lvl, ushort count)
        {
            AddToAttack((ushort)type, lvl, count, FormationType.Normal);
        }

        public void AddToAttack(ushort type, byte lvl, ushort count, FormationType formation)
        {
            using (Concurrency.Current.Lock(city))
            {
                city.BeginUpdate();
                city.Template[type] = Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type, lvl);
                if (city.Template[type] == null)
                {
                    throw new Exception("Unit type not found!");
                }
                city.EndUpdate();
                AttackStub.BeginUpdate();
                AttackStub.AddUnit(formation, type, count);
                AttackStub.EndUpdate();
            }
        }

        public int Upkeep(UnitType type)
        {
            return AttackStub[FormationType.Normal][(ushort)type] * city.Template[(ushort)type].Upkeep;
        }

        public int Upkeep()
        {
            return city.DefaultTroop.Upkeep;
        }
    }
}