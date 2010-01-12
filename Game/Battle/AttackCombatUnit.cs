#region

using System;
using System.Data;
using Game.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Fighting;
using Game.Setup;

#endregion

namespace Game.Battle {
    public class AttackCombatUnit : CombatObject, ICombatUnit, IComparable<object> {
        private TroopStub stub;
        private FormationType formation;
        private BattleStats stats;
        private byte lvl;
        private ushort type;
        private ushort count;
        private ushort hp;

        public ushort Hp {
            get { return hp; }
            set { hp = value; }
        }

        public TroopStub TroopStub {
            get { return stub; }
        }

        public override BattleClass ClassType {
            get { return BattleClass.Unit; }
        }

        public Resource Loot {
            get { return TroopStub.TroopObject.Stats.Loot; }
        }

        public override bool IsDead {
            get { return HP == 0; }
        }

        public override ushort Count {
            get { return count; }
        }

        public override ushort Type {
            get { return type; }
        }

        public override BaseBattleStats BaseStats {
            get { return UnitFactory.getBattleStats(type, lvl); }
        }

        public override BattleStats Stats {
            get { return stats; }
        }

        public FormationType Formation {
            get { return formation; }
        }

        public AttackCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, byte lvl,
                                ushort count) {
            this.stub = stub;
            this.formation = formation;
            this.type = type;
            this.count = count;
            battleManager = owner;
            this.lvl = lvl;

            stats = stub.Template[type];
            Hp = stats.MaxHp;
        }

        public override bool InRange(CombatObject obj) {
            if (obj is AttackCombatUnit || obj is DefenseCombatUnit) //all units can attack other units
                return true;

            int dist = obj.Distance(stub.TroopObject.X, stub.TroopObject.Y);

            return dist <= stub.TroopObject.Stats.AttackRadius;
        }

        public override int Distance(uint x, uint y) {
            return GameObject.Distance(x, y, stub.TroopObject.X, stub.TroopObject.Y);
        }

        public override uint Visibility {
            get {
                if (formation == FormationType.Scout)
                    return (uint) (RoundsParicipated*1.5 + Stats.Rng);
                else
                    return (uint) (RoundsParicipated + Stats.Rng);
            }
        }

        public override uint PlayerId {
            get { return stub.City.Owner.PlayerId; }
        }

        public override City City {
            get { return stub.City; }
        }

        public override byte Lvl {
            get { return lvl; }
        }

        public override uint HP {
            get { return (uint) (Math.Max(0, stats.MaxHp*(count - 1) + Hp)); }
        }

        public override void CalculateDamage(ushort dmg, out int actualDmg) {
            actualDmg = Math.Min((int) HP, dmg);
/*            if (dmg >= stats.Hp) {
                actualDmg += stats.Hp;
            }*/
        }

        public override void TakeDamage(int dmg) {
            ushort dead = 0;
            if (dmg >= Hp) {
                dmg -= Hp;
                Hp = (ushort) (stats.MaxHp);
                dead++;
            }

            dead += (ushort) (dmg/stats.MaxHp);
            Hp -= (ushort) (dmg%stats.MaxHp);

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

        public override void ExitBattle() {}

        public override void ReceiveReward(int reward, Resource resource) {
            stub.TroopObject.BeginUpdate();
            stub.TroopObject.Stats.RewardPoint += reward;
            stub.TroopObject.Stats.Loot.add(resource);
            stub.TroopObject.EndUpdate();
        }

        #region IComparable<GameObject> Members

        public override int CompareTo(object other) {
            if (other is TroopStub) {
                if (other == stub)
                    return 0;
                else
                    return 1;
            } else
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
                                          new DbColumn("id", Id, DbType.UInt32),
                                          new DbColumn("city_id", battleManager.City.CityId, DbType.UInt32)
                                      };
            }
        }

        public override DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                                          new DbColumn("last_round", LastRound, DbType.UInt32),
                                          new DbColumn("rounds_participated", RoundsParicipated, DbType.UInt32),
                                          new DbColumn("damage_dealt", DmgDealt, DbType.Int32),
                                          new DbColumn("damage_received", DmgRecv, DbType.Int32),
                                          new DbColumn("group_id", GroupId, DbType.UInt32),
                                          new DbColumn("formation_type", (byte) formation, DbType.Byte),
                                          new DbColumn("level", lvl, DbType.Byte), new DbColumn("count", count, DbType.UInt16),
                                          new DbColumn("type", type, DbType.UInt16), new DbColumn("is_local", false, DbType.Boolean),
                                          new DbColumn("troop_stub_city_id", stub.City.CityId, DbType.UInt32),
                                          new DbColumn("troop_stub_id", stub.TroopId, DbType.Byte),
                                      };
            }
        }

        #endregion
    }
}