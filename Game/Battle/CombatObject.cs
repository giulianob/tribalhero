using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Fighting;
using Game.Database;

namespace Game.Battle {
    public enum BattleClass : byte {
        Structure = 0,
        Unit = 1
    }

    public abstract class CombatObject : IComparable<object>, IPersistableObject {
        int maxDmgRecv;
        public int MaxDmgRecv {
            get { return maxDmgRecv; }
            set { maxDmgRecv = value; }
        }

        int minDmgRecv = int.MaxValue;
        public int MinDmgRecv {
            get { return minDmgRecv; }
            set { minDmgRecv = value; }
        }

        int maxDmgDealt;
        public int MaxDmgDealt {
            get { return maxDmgDealt; }
            set { maxDmgDealt = value; }
        }

        int minDmgDealt = int.MaxValue;
        public int MinDmgDealt {
            get { return minDmgDealt; }
            set { minDmgDealt = value; }
        }

        int hitRecv = 0;
        public int HitRecv {
            get { return hitRecv; }
            set { hitRecv = value; }
        }

        int hitDealt = 0;
        public int HitDealt {
            get { return hitDealt; }
            set { hitDealt = value; }
        }

        int dmgRecv = 0;
        public int DmgRecv {
            get { return dmgRecv; }
            set { dmgRecv = value; }
        }

        int dmgDealt = 0;
        public int DmgDealt {
            get { return dmgDealt; }
            set { dmgDealt = value; }
        }

        CombatList combatList;
        public CombatList CombatList
        {
            get { return combatList; }
            set { combatList = value; }
        }

        int roundsParticipated = 0;
        public int RoundsParicipated {
            get { return roundsParticipated; }
            set { roundsParticipated = value; }
        }

        uint lastRound = 0;
        public uint LastRound {
            get { return lastRound; }
            set { lastRound = value; }
        }

        uint id = 0;
        public uint Id {
            get { return id; }
            set { id = value; }
        }

        uint groupId = 0; //Unique per 'group' of combat objects (ie: all units in the same troop will share the same id
        public uint GroupId {
            get { return groupId; }
            set { groupId = value; }
        }

        protected BattleManager battleManager = null;
        public BattleManager Battle {
            get { return battleManager; }
        }

        public virtual void CleanUp() { throw new Exception("NOT IMPLEMENTED"); }
        public virtual void ExitBattle() { throw new Exception("NOT IMPLEMENTED"); }
        public virtual void TakeDamage(int dmg) { throw new Exception("NOT IMPLEMENTED"); }
        public virtual void CalculateDamage(ushort dmg, out int actualDmg) { throw new Exception("NOT IMPLEMENTED"); }
        public virtual bool InRange(CombatObject obj) { throw new Exception("NOT IMPLEMENTED"); }       

        public virtual int Distance(uint x, uint y) { throw new Exception("NOT IMPLEMENTED"); }
        public virtual void ReceiveReward(int reward, Resource resource) { throw new Exception("NOT IMPLEMENTED"); }

        public virtual bool IsDead {
            get { throw new Exception("NOT IMPLEMENTED"); }
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
        public virtual uint HP {
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
            int roundsDelta = Math.Min(roundsParticipated, obj.RoundsParicipated);
            return Visibility >= obj.Stats.Stl;
        }

        public void ParticipatedInRound()
        {
            lastRound++;
            roundsParticipated++;
            Global.dbManager.Save(this);
        }

        public void Print() {
            System.Console.WriteLine("recv[{0}] dealt[{1}] hitRecv[{2}] hitDealt[{3}] maxDealt[{4}] minDealt[{5}] maxRecv[{6}] minRecv[{7}]", DmgRecv, DmgDealt, HitRecv, HitDealt, maxDmgDealt, MinDmgDealt, maxDmgRecv, MaxDmgRecv);
        }

        #region IComparable<GameObject> Members

        public virtual int CompareTo(object other) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion



        #region IPersistableObject Members
        bool dbPersisted = false;
        public bool DbPersisted {
            get {
                return dbPersisted;
            }
            set {
                dbPersisted = value;
            }
        }

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