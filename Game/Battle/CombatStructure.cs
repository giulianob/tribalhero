#region

using System;
using System.Data;
using Game.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Logic.Procedures;

#endregion

namespace Game.Battle {
    public class CombatStructure : CombatObject {
        private readonly byte lvl;
        private readonly ushort type;
        private uint hp; //need to keep a copy track of the hp for reporting
        private readonly BattleStats stats;

        public Structure Structure { get; private set; }

        public override BaseBattleStats BaseStats {
            get { return Structure.Stats.Base.Battle; }
        }

        public override BattleStats Stats {
            get { return stats; }
        }

        public CombatStructure(BattleManager owner, Structure structure, BattleStats stats) {
            battleManager = owner;
            this.stats = stats;
            this.Structure = structure;
            type = structure.Type;
            lvl = structure.Lvl;
            hp = structure.Stats.Hp;
        }

        public CombatStructure(BattleManager owner, Structure structure, BattleStats stats, uint hp, ushort type,
                               byte lvl) {
            battleManager = owner;
            this.Structure = structure;
            this.stats = stats;
            this.hp = hp;
            this.type = type;
            this.lvl = lvl;
        }

        public override bool InRange(CombatObject obj) {
            int dist;

            if (obj is AttackCombatUnit) {
                //building can attack anyone who can attack them
                dist = obj.Distance(Structure.X, Structure.Y);
                if (dist <= (obj as AttackCombatUnit).TroopStub.TroopObject.Stats.AttackRadius)
                    return true;
            } else if (obj is DefenseCombatUnit)
                return true;

            dist = obj.Distance(Structure.X, Structure.Y);

            return dist <= Stats.Rng;
        }

        public override int Distance(uint x, uint y) {
            return GameObject.Distance(x, y, Structure.X, Structure.Y);
        }

        public override uint Visibility {
            get { return (uint) (RoundsParticipated + Stats.Rng); }
        }

        public override uint PlayerId {
            get { return Structure.City.Owner.PlayerId; }
        }

        public override City City {
            get { return Structure.City; }
        }

        public override BattleClass ClassType {
            get { return BattleClass.STRUCTURE; }
        }

        public override bool IsDead {
            get { return hp == 0; }
        }

        public override ushort Count {
            get { return (ushort) (hp > 0 ? 1 : 0); }
        }

        public override byte Lvl {
            get { return lvl; }
        }

        public override ushort Type {
            get { return type; }
        }

        public override uint Hp {
            get { return hp; }
        }

        public override void CalculateDamage(ushort dmg, out ushort actualDmg) {
            actualDmg = dmg;
        }

        public override void TakeDamage(int dmg, out Resource returning) {
            Structure.BeginUpdate();
            Structure.Stats.Hp = (dmg > Structure.Stats.Hp) ? (ushort)0 : (ushort)(Structure.Stats.Hp - (ushort)dmg);            
            Structure.EndUpdate();

            hp = (dmg > hp) ? 0 : hp - (ushort)dmg;
            
            returning = null;
        }

        public override void CleanUp() {
            if (hp <= 0) {
                City city = Structure.City;

                Global.World.LockRegion(Structure.X, Structure.Y);
                if (Structure.Lvl > 1) {
                    Structure.City.BeginUpdate();
                    Structure.BeginUpdate();
                    Procedure.StructureDowngrade(Structure);
                    Structure.State = GameObjectState.NormalState();
                    Structure.EndUpdate();
                    Structure.City.EndUpdate();
                } else {
                    Global.World.Remove(Structure);
                    city.Remove(Structure);
                }
                Global.World.UnlockRegion(Structure.X, Structure.Y);
            }

            Global.DbManager.Delete(this);
        }

        public override void ExitBattle() {
            Structure.BeginUpdate();
            Structure.State = GameObjectState.NormalState();
            Structure.EndUpdate();
        }

        public override void ReceiveReward(int reward, Resource resource) {
            return;
        }

        #region IComparable<GameObject> Members

        public override int CompareTo(object other) {
            if (other is Structure) {
                return other == Structure ? 0 : 1;
            }

            return -1;
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "combat_structures";

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
                                          new DbColumn("damage_dealt", DmgDealt, DbType.Int32),
                                          new DbColumn("damage_received", DmgRecv, DbType.Int32),
                                          new DbColumn("group_id", GroupId, DbType.UInt32),
                                          new DbColumn("structure_city_id", Structure.City.Id, DbType.UInt32),
                                          new DbColumn("structure_id", Structure.ObjectId, DbType.UInt32),
                                          new DbColumn("hp", hp, DbType.UInt16), new DbColumn("type", type, DbType.UInt16),
                                          new DbColumn("level", lvl, DbType.Byte), //BattleStats
                                          new DbColumn("max_hp", stats.MaxHp, DbType.UInt16),
                                          new DbColumn("attack", stats.Atk, DbType.UInt16), new DbColumn("defense", stats.Def, DbType.UInt16),
                                          new DbColumn("range", stats.Rng, DbType.Byte), new DbColumn("stealth", stats.Stl, DbType.Byte),
                                          new DbColumn("speed", stats.Spd, DbType.Byte)
                                      };
            }
        }

        #endregion
    }
}