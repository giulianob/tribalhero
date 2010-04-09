#region

using System;
using Game.Data;
using Game.Data.Stats;
using Game.Database;

#endregion

namespace Game.Battle {
    public enum BattleClass : byte {
        STRUCTURE = 0,
        UNIT = 1
    }

    public abstract class CombatObject : IComparable<object>, IPersistableObject {
        public ushort MaxDmgRecv { get; set; }
        public ushort MinDmgRecv { get; set; }

        public ushort MaxDmgDealt { get; set; }
        public ushort MinDmgDealt { get; set; }

        public ushort HitRecv { get; set; }
        public ushort HitDealt { get; set; }

        public int DmgRecv { get; set; }
        public int DmgDealt { get; set; }

        public CombatList CombatList { get; set; }

        public int RoundsParticipated { get; set; }

        public uint LastRound { get; set; }

        public uint Id { get; set; }

        public uint GroupId { get; set; }

        protected BattleManager battleManager;

        protected CombatObject() {
            MinDmgDealt = ushort.MaxValue;
            MinDmgRecv = ushort.MaxValue;
        }

        public BattleManager Battle {
            get { return battleManager; }
        }

        public virtual void CleanUp() {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void ExitBattle() {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void TakeDamage(int dmg, out Resource returning, out int attackPoints) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void CalculateDamage(ushort dmg, out ushort actualDmg) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual bool InRange(CombatObject obj) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual int TileDistance(uint x, uint y) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void ReceiveReward(int reward, Resource resource) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual bool IsDead {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual BaseBattleStats BaseStats {
            get { throw new Exception("NOT IMPLEMENTED"); }
            set { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual BattleStats Stats {
            get { throw new Exception("NOT IMPLEMENTED"); }
            set { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual ushort Type {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual BattleClass ClassType {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual ushort Count {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual uint Hp {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual uint Visibility {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual uint PlayerId {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual City City {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public virtual byte Lvl {
            get { throw new Exception("NOT IMPLEMENTED"); }
        }

        public bool CanSee(CombatObject obj) {
            return Visibility >= obj.Stats.Stl;
        }

        public void ParticipatedInRound() {
            LastRound++;
            RoundsParticipated++;            
        }

        #region IComparable<GameObject> Members

        public virtual int CompareTo(object other) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public virtual string DbTable {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public virtual DbColumn[] DbPrimaryKey {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public virtual DbDependency[] DbDependencies {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public virtual DbColumn[] DbColumns {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion
    }
}