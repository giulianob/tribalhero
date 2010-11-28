#region

using System;
using System.Data;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Database;
using Game.Fighting;
using Game.Logic;
using Game.Setup;

#endregion

namespace Game.Battle {
    public class DefenseCombatUnit : CombatObject, ICombatUnit {
        private readonly FormationType formation;
        private readonly BattleStats stats;

        private readonly byte lvl;
        private readonly ushort type;
        private ushort count;

        public ushort LeftOverHp { set; get; }

        public TroopStub TroopStub { get; private set; }

        public override BattleClass ClassType {
            get { return BattleClass.UNIT; }
        }

        public override bool IsDead {
            get { return Hp == 0; }
        }

        public override ushort Count {
            get { return count; }
        }

        public override ushort Type {
            get { return type; }
        }

        public override BaseBattleStats BaseStats {
            get { return UnitFactory.GetBattleStats(type, lvl); }
        }

        public override BattleStats Stats {
            get { return stats; }
        }

        public FormationType Formation {
            get { return formation; }
        }

        public Resource Loot {
            get { return new Resource(); }
        }

        public override int Upkeep
        {
            get { return UnitFactory.GetUnitStats(type, lvl).Upkeep * count; }
        }

        public override short Stamina
        {
            get { return -1; }
        }

        public DefenseCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, byte lvl,
                                 ushort count) {
            TroopStub = stub;
            this.formation = formation;
            this.type = type;
            this.count = count;
            battleManager = owner;
            this.lvl = lvl;

            stats = stub.Template[type];
            LeftOverHp = stats.MaxHp;
        }

        public DefenseCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, byte lvl,
                         ushort count, ushort leftOverHp)
            : this(owner, stub, formation, type, lvl, count) {
            LeftOverHp = leftOverHp;
        }

        public override bool InRange(CombatObject obj) {
            return true;
        }

        public override int TileDistance(uint x, uint y) {
            return 0;
        }

        public override uint Visibility {
            get {                
                return Stats.Rng;
            }
        }

        public override uint PlayerId {
            get { return TroopStub.City.Owner.PlayerId; }
        }

        public override City City {
            get { return TroopStub.City; }
        }

        public override byte Lvl {
            get { return lvl; }
        }

        public override uint Hp {
            get { return (uint) (Math.Max(0, stats.MaxHp*(count - 1) + LeftOverHp)); }
        }

        public override void CalculateDamage(ushort dmg, out ushort actualDmg) {
            actualDmg = (ushort)Math.Min(Hp, dmg);
        }

        public override void TakeDamage(int dmg, out Resource returning, out int attackPoints) {
            attackPoints = 0;

            ushort dead = 0;
            if (dmg >= LeftOverHp) {
                dmg -= LeftOverHp;
                LeftOverHp = stats.MaxHp;
                dead++;
            }

            dead += (ushort) (dmg/stats.MaxHp);
            LeftOverHp -= (ushort) (dmg%stats.MaxHp);

            if (dead > 0) {
                if (dead > count)
                    dead = count;
                
                count -= dead;

                attackPoints = Formula.GetUnitKilledAttackPoint(type, lvl, dead);

                // Remove dead units from troop stub
                TroopStub.BeginUpdate();
                TroopStub[formation].Remove(type, dead);
                TroopStub.EndUpdate();
            }
            
            returning = null;
        }

        public override void CleanUp() {
            Global.DbManager.Delete(this);
        }

        public override void ExitBattle() {}

        #region IComparable<GameObject> Members

        public override int CompareTo(object other) {
            if (other is TroopStub) {
                return other == TroopStub ? 0 : 1;
            }

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
                return new[] {
                                  new DbColumn("id", Id, DbType.UInt32),
                                  new DbColumn("city_id", battleManager.City.Id, DbType.UInt32)
                              };
            }
        }

        public override DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public override DbColumn[] DbColumns {
            get {
                return new[] {
                                  new DbColumn("last_round", LastRound, DbType.UInt32),
                                  new DbColumn("rounds_participated", RoundsParticipated, DbType.UInt32),

                                  new DbColumn("group_id", GroupId, DbType.UInt32),
                                  new DbColumn("formation_type", (byte) formation, DbType.Byte),
                                  new DbColumn("level", lvl, DbType.Byte), new DbColumn("count", count, DbType.UInt16),
                                  new DbColumn("type", type, DbType.UInt16), new DbColumn("is_local", true, DbType.Boolean),
                                  
                                  new DbColumn("troop_stub_city_id", TroopStub.City.Id, DbType.UInt32),
                                  new DbColumn("troop_stub_id", TroopStub.TroopId, DbType.Byte),
                                  
                                  new DbColumn("left_over_hp", LeftOverHp, DbType.UInt16),

                                  new DbColumn("damage_min_dealt", MinDmgDealt, DbType.UInt16),
                                  new DbColumn("damage_max_dealt", MaxDmgDealt, DbType.UInt16),

                                  new DbColumn("damage_min_received", MinDmgRecv, DbType.UInt16),
                                  new DbColumn("damage_max_received", MaxDmgRecv, DbType.UInt16),
                                  
                                  new DbColumn("damage_dealt", DmgDealt, DbType.Int32),
                                  new DbColumn("damage_received", DmgRecv, DbType.Int32),

                                  new DbColumn("hits_dealt", HitDealt, DbType.UInt16),
                                  new DbColumn("hits_dealt_by_unit", HitDealtByUnit, DbType.UInt32),
                                  new DbColumn("hits_received", HitRecv, DbType.UInt16),                                                                                                
                            };
            }
        }

        #endregion
    }
}