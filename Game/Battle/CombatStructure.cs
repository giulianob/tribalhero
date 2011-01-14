#region

using System;
using System.Data;
using Game.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
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
            Structure = structure;
            type = structure.Type;
            lvl = structure.Lvl;
            hp = structure.Stats.Hp;
        }

        public CombatStructure(BattleManager owner, Structure structure, BattleStats stats, uint hp, ushort type,
                               byte lvl) {
            battleManager = owner;
            Structure = structure;
            this.stats = stats;
            this.hp = hp;
            this.type = type;
            this.lvl = lvl;
        }

        public override bool InRange(CombatObject obj) {
            if (obj is AttackCombatUnit) {
                TroopObject troop = (obj as AttackCombatUnit).TroopStub.TroopObject;
                return troop.RadiusDistance(Structure) <= Structure.Stats.Base.Radius + troop.Stats.AttackRadius;
            }

            throw new Exception(string.Format("Why is a structure trying to kill a unit of type {0}?", obj.GetType().FullName));
        }

        public override int TileDistance(uint x, uint y) {
            return SimpleGameObject.TileDistance(x, y, Structure.X, Structure.Y);
        }

        public override uint Visibility {
            get { return Stats.Rng; }
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

        public override int Upkeep {
            get { return BattleFormulas.GetUnitsPerStructure(Structure); }
        }

        public override short Stamina
        {
            get { return -1; }
        }
        
        public override void CalculateDamage(ushort dmg, out ushort actualDmg) {
            actualDmg = (ushort) Math.Min(Hp, dmg);
        }

        public override void TakeDamage(int dmg, out Resource returning, out int attackPoints) {
            attackPoints = 0;

            Structure.BeginUpdate();
            Structure.Stats.Hp = (dmg > Structure.Stats.Hp) ? (ushort)0 : (ushort)(Structure.Stats.Hp - (ushort)dmg);            
            Structure.EndUpdate();

            hp = (dmg > hp) ? 0 : hp - (ushort)dmg;

            if (hp == 0)
                attackPoints = Formula.GetStructureKilledAttackPoint(type, lvl);                
            
            returning = null;
        }

        public override void CleanUp() {
            if (hp <= 0) {
                City city = Structure.City;

                Global.World.LockRegion(Structure.X, Structure.Y);
                if (Structure.Lvl > 1) {                    
                    Structure.BeginUpdate();                    
                    Structure.State = GameObjectState.NormalState();
                    Structure.EndUpdate();

                    Structure.City.Worker.DoPassive(Structure.City, new StructureDowngradeAction(Structure.City.Id, Structure.ObjectId), false);
                } else {
                    Structure.BeginUpdate();
                    Global.World.Remove(Structure);                   
                    city.ScheduleRemove(Structure, true);
                    Structure.EndUpdate();
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
                                            new DbColumn("speed", stats.Spd, DbType.Byte),
                                            new DbColumn("hits_dealt", HitDealt, DbType.UInt16),
                                            new DbColumn("hits_dealt_by_unit", HitDealtByUnit, DbType.UInt32),
                                            new DbColumn("hits_received", HitRecv, DbType.UInt16),       
                                      };
            }
        }

        #endregion
    }
}