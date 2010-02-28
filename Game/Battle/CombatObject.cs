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
        private int maxDmgRecv;

        public int MaxDmgRecv {
            get { return maxDmgRecv; }
            set { maxDmgRecv = value; }
        }

        private int minDmgRecv = int.MaxValue;

        public int MinDmgRecv {
            get { return minDmgRecv; }
            set { minDmgRecv = value; }
        }

        private int maxDmgDealt;

        public int MaxDmgDealt {
            get { return maxDmgDealt; }
            set { maxDmgDealt = value; }
        }

        private int minDmgDealt = int.MaxValue;

        public int MinDmgDealt {
            get { return minDmgDealt; }
            set { minDmgDealt = value; }
        }

        public int HitRecv { get; set; }

        public int HitDealt { get; set; }

        public int DmgRecv { get; set; }

        public int DmgDealt { get; set; }

        public CombatList CombatList { get; set; }

        public int RoundsParticipated { get; set; }

        private uint lastRound;

        public uint LastRound {
            get { return lastRound; }
            set { lastRound = value; }
        }

        public uint Id { get; set; }

        public uint GroupId { get; set; }

        protected BattleManager battleManager;

        public BattleManager Battle {
            get { return battleManager; }
        }

        public virtual void CleanUp() {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void ExitBattle() {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void TakeDamage(int dmg, out Resource returning) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void CalculateDamage(ushort dmg, out int actualDmg) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual bool InRange(CombatObject obj) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual int Distance(uint x, uint y) {
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
            lastRound++;
            RoundsParticipated++;
            Global.DbManager.Save(this);
        }

        public void Print() {
            Console.WriteLine(
                "recv[{0}] dealt[{1}] hitRecv[{2}] hitDealt[{3}] maxDealt[{4}] minDealt[{5}] maxRecv[{6}] minRecv[{7}]",
                DmgRecv, DmgDealt, HitRecv, HitDealt, maxDmgDealt, MinDmgDealt, maxDmgRecv, MaxDmgRecv);
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