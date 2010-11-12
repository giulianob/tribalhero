#region

using System;
using Game.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Setup;

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
        public uint HitDealtByUnit { get; set; }

        public int DmgRecv { get; set; }
        public int DmgDealt { get; set; }

        public abstract int Upkeep { get; }

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
            // In order to implement visibility as discussed in http://trac.tribalhero.com/game/wiki/Discussion%2011/08/10
            // we always take the lowest RoundsParticipated of the two objects.
            int minRoundsParticipated = Math.Min(RoundsParticipated, obj.RoundsParticipated);
            int totalVision = (int) (Visibility + minRoundsParticipated);        
            switch(obj.BaseStats.Weapon) {
                case WeaponType.BOW:
                    totalVision += (minRoundsParticipated * BaseStats.Spd / 15);
                    break;
                default:
                    break;
            }
            
            if (totalVision >= obj.Stats.Stl) {
                return true;
            }

            // if vision < stealth by 1, u have 33% chance 
            // if vision < stealth by 2, u have 25% chance 
            // if vision < stealth by 3, u have 20% chance 
            // if vision < stealth by 4, u have 16% chance 
            // if vision < stealth by 5, u have 14% chance 
            // if vision < stealth by 6, u have 12% chance 
            // if vision < stealth by 7, u have 14% chance 
            // if vision < stealth by 8, u have 10% chance 
            // if vision < stealth by 9, u have 9% chance 
            if (Config.Random.Next(obj.Stats.Stl - totalVision + 1) == 0)  
                return true;
            return false;
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


        public void Print() {
          //  throw new NotImplementedException();
        }
    }
}