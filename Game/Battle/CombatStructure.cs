using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Util;
using Game.Database;
using Game.Setup;
using Game.Data.Stats;

namespace Game.Battle {
    public class CombatStructure: CombatObject, IComparable<object>, IPersistableObject {
        Structure structure;
        byte lvl;
        ushort type;
        uint hp; //need to keep a copy track of the hp for reporting
        BattleStats stats;

        public Structure Structure {
            get { return structure; }
        }

        public override BaseBattleStats BaseStats {
            get {
                return structure.Stats.Base.Battle;
            }
        }

        public override BattleStats Stats {
            get { return stats; }
        }

        public CombatStructure(BattleManager owner, Structure structure, BattleStats stats) {
            this.battleManager = owner;
            this.stats = stats;
            this.structure = structure;
            this.type = structure.Type;
            this.lvl = structure.Lvl;
            this.hp = structure.Stats.Hp;
        }

        public CombatStructure(BattleManager owner, Structure structure, BattleStats stats, uint hp, ushort type, byte lvl) {
            this.battleManager = owner;
            this.structure = structure;
            this.stats = stats;
            this.hp = hp;
            this.type = type;
            this.lvl = lvl;
        }

        public override bool InRange(CombatObject obj) {
            int dist;

            if (obj is AttackCombatUnit) { //building can attack anyone who can attack them
                dist = obj.Distance(structure.X, structure.Y);
                if (dist <= (obj as AttackCombatUnit).TroopStub.TroopObject.Stats.AttackRadius)
                    return true;
            }
            else if (obj is DefenseCombatUnit)
                return true;

            dist = obj.Distance(structure.X, structure.Y);

            return dist <= Stats.Rng;
        }

        public override int Distance(uint x, uint y) {
            return GameObject.distance(x, y, structure.X, structure.Y);
        }

        public override uint Visibility {
            get { return (uint)(RoundsParicipated + Stats.Rng); }
        }

        public override uint PlayerId {
            get {
                return structure.City.Owner.PlayerId;
            }
        }
        public override City City {
            get {
                return structure.City;
            }
        }

        public override BattleClass ClassType {
            get {
                return BattleClass.Structure;
            }
        }

        public override bool IsDead {
            get { return hp == 0; }
        }

        public override ushort Count {
            get {
                return (ushort)(hp > 0 ? 1 : 0);
            }
        }
        public override byte Lvl {
            get {
                return lvl;
            }
        }

        public override ushort Type {
            get {
                return type;
            }
        }

        public override uint HP {
            get {
                return hp;
            }
        }
        public override void CalculateDamage(ushort dmg, out int actualDmg) {
            actualDmg = (int)Math.Min(hp, dmg);
        }

        public override void TakeDamage(int Dmg)
        {
            structure.BeginUpdate();
            structure.Stats.Hp -= (ushort)Dmg;
            if (structure.Stats.Hp < 0) structure.Stats.Hp = 0;

            hp -= (ushort)Dmg;
            if (hp < 0) hp = 0;
            structure.EndUpdate();

            Global.dbManager.Save(this);
        }

        public override void CleanUp() {
            if (hp <= 0) {
                City city = structure.City;

                Global.World.lockRegion(structure.X, structure.Y);
                if (structure.Lvl > 1) {
                    structure.BeginUpdate();
                    Procedure.StructureDowngrade(structure);
                    structure.State = GameObjectState.NormalState();
                    structure.EndUpdate();
                }
                else {
                    Global.World.remove(structure);
                    city.remove(structure);
                }
                Global.World.unlockRegion(structure.X, structure.Y);
            }

            Global.dbManager.Delete(this);
        }

        public override void ExitBattle()
        {
            structure.BeginUpdate();
            structure.State = GameObjectState.NormalState();
            structure.EndUpdate();
        }

        public override void ReceiveReward(int reward, Resource resource) {
            return;
        }

        #region IComparable<GameObject> Members

        public override int CompareTo(object other) {
            if (other is Structure) {
                if (other == structure)
                    return 0;
                else
                    return 1;
            }
            else {
                return -1;
            }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "combat_structures";

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
                    new DbColumn("structure_city_id", structure.City.CityId, System.Data.DbType.UInt32),
                    new DbColumn("structure_id", structure.ObjectID, System.Data.DbType.UInt32),
                    new DbColumn("hp", hp, System.Data.DbType.UInt16),
                    new DbColumn("type", type, System.Data.DbType.UInt16),
                    new DbColumn("level", lvl, System.Data.DbType.Byte),
                    //BattleStats
                    new DbColumn("max_hp", stats.MaxHp, System.Data.DbType.UInt16),
                    new DbColumn("attack", stats.Atk, System.Data.DbType.Byte),
                    new DbColumn("defense", stats.Def, System.Data.DbType.Byte),
                    new DbColumn("range", stats.Rng, System.Data.DbType.Byte),
                    new DbColumn("stealth", stats.Stl, System.Data.DbType.Byte),
                    new DbColumn("speed", stats.Spd, System.Data.DbType.Byte),
                    new DbColumn("reward", stats.Reward, System.Data.DbType.UInt16)
                };
            }
        }

        #endregion
    }
}
