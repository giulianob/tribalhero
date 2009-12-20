using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Fighting;
using Game.Setup;
using Game.Util;
using Game.Database;
using Game.Data.Stats;

namespace Game.Battle {
    public class DefenseCombatUnit: CombatObject, IComparable<object>, ICombatUnit {

        TroopStub stub;
        FormationType formation;
        BattleStats stats;

        byte lvl;
        ushort type;
        ushort count;
        ushort hp;
        public ushort Hp {
            set { hp = value; }
            get { return hp; }
        }

        public TroopStub TroopStub {
            get { return stub; }
        }

        public override BattleClass ClassType {
            get {
                return BattleClass.Unit;
            }
        }

        public override bool IsDead {
            get { return HP == 0; }
        }

        public override ushort Count {
            get {
                return count;
            }
        } 

        public override ushort Type {
            get {
                return type;
            }
        }

        public override BaseBattleStats BaseStats {
            get {
                return UnitFactory.getBattleStats(type, lvl);
            }
        }

        public override BattleStats Stats {
            get {
                return stats;
            }
        }

        public FormationType Formation {
            get { return formation; }
        }

        public Resource Loot {
            get { return new Resource(); }
        }

        public DefenseCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, byte lvl, ushort count) {
            this.stub = stub;
            this.formation = formation;
            this.type = type;            
            this.count = count;
            this.battleManager = owner;
            this.lvl = lvl;
            
            stats = stub.Template[type];
            hp = stats.MaxHp;
        }

        public override bool InRange(CombatObject obj) {
            return true;
        }

        public override int Distance(uint x, uint y) {
            return 0;
        }

        public override uint Visibility {
            get
            {
                if (formation == FormationType.Scout)
                    return (uint)(RoundsParicipated * 1.5 + Stats.Rng);
                else
                    return (uint)(RoundsParicipated + Stats.Rng);
            }
        }

        public override uint PlayerId {
            get {
                return stub.City.Owner.PlayerId;
            }
        }

        public override City City {
            get {
                return stub.City;
            }
        }

        public override byte Lvl {
            get {
                return lvl;
            }
        }

        public override uint HP {
            get {
                return (uint)(Math.Max(0, stats.MaxHp * (count-1) + Hp));
            }
        }

        public override void CalculateDamage(ushort dmg, out int actualDmg) {
            actualDmg = Math.Min((int)HP, dmg);
        }

        public override void TakeDamage(int dmg) {
            ushort dead = 0;
            if (dmg >= Hp) {
                dmg -= Hp;
                Hp = (ushort)(stats.MaxHp);
                dead++;
            }

            dead += (ushort)(dmg / stats.MaxHp);
            Hp -= (ushort)(dmg % stats.MaxHp);

            if (dead > 0) {
                if (dead >= count)
                    count = 0;
                else
                    count -= dead;

                stub.BeginUpdate();
                stub[formation].remove(type, dead);
                stub.EndUpdate();
            }

            Global.dbManager.Save(this);
        }

        public override void CleanUp() {
            Global.dbManager.Delete(this);
        }

        public override void ExitBattle()
        {

        }

        #region IComparable<GameObject> Members

        public override int CompareTo(object other) {
            if (other is TroopStub) {
                if (other == stub)
                    return 0;
                else
                    return 1;
            }
            else
                return -1;
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "combat_units";

        public override string DbTable {
            get { return DB_TABLE; }
        }

        public override DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("id", Id, System.Data.DbType.UInt32),
                    new DbColumn("city_id", battleManager.City.CityId, System.Data.DbType.UInt32)
                };
            }
        }

        public override DbDependency[] DbDependencies {
            get { return new DbDependency[] { }; }
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("last_round", LastRound, System.Data.DbType.UInt32),
                    new DbColumn("rounds_participated", RoundsParicipated, System.Data.DbType.UInt32),
                    new DbColumn("damage_dealt", DmgDealt, System.Data.DbType.Int32),
                    new DbColumn("damage_received", DmgRecv, System.Data.DbType.Int32),
                    new DbColumn("group_id", GroupId, System.Data.DbType.UInt32),
                    new DbColumn("formation_type", (byte)formation, System.Data.DbType.Byte),
                    new DbColumn("level", lvl, System.Data.DbType.Byte),
                    new DbColumn("count", count, System.Data.DbType.UInt16),
                    new DbColumn("type", type, System.Data.DbType.UInt16),
                    new DbColumn("is_local", true, System.Data.DbType.Boolean),
                    new DbColumn("troop_stub_city_id", stub.City.CityId, System.Data.DbType.UInt32),
                    new DbColumn("troop_stub_id", stub.TroopId, System.Data.DbType.Byte),
                };
            }
        }

        #endregion
    }
}
