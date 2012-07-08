#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Database;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Battle
{
    public abstract class CombatObject : IComparable<object>, IPersistableObject, ILockable
    {
        protected readonly uint BattleId;

        protected CombatObject(uint battleId)
        {
            MinDmgDealt = ushort.MaxValue;
            MinDmgRecv = ushort.MaxValue;
            BattleId = battleId;
        }

        #region Properties

        public ushort MaxDmgRecv { get; set; }
        public ushort MinDmgRecv { get; set; }

        public ushort MaxDmgDealt { get; set; }
        public ushort MinDmgDealt { get; set; }

        public ushort HitRecv { get; set; }
        public ushort HitDealt { get; set; }
        public uint HitDealtByUnit { get; set; }

        public decimal DmgRecv { get; set; }
        public decimal DmgDealt { get; set; }        

        public CombatList CombatList { get; set; }

        public int RoundsParticipated { get; set; }

        public uint LastRound { get; set; }

        public uint Id { get; set; }

        public uint GroupId { get; set; }

        public bool Disposed { get; private set; }

        #endregion

        #region Abstract Properties

        public abstract int Upkeep { get; }

        public abstract bool IsDead { get; }

        public abstract BattleStats Stats { get; }

        public abstract ushort Type { get; }

        public abstract Resource Loot { get; }

        public abstract Resource GroupLoot { get; }

        public abstract ITroopStub TroopStub { get; }

        public abstract BattleClass ClassType { get; }

        public abstract ushort Count { get; }

        public abstract decimal Hp { get; }

        public abstract uint Visibility { get; }

        public abstract uint PlayerId { get; }

        public abstract ICity City { get; }

        public abstract byte Lvl { get; }

        #endregion

        #region Abstract Methods

        public abstract void ExitBattle();

        public abstract void TakeDamage(decimal dmg, out Resource returning, out int attackPoints);

        public abstract void CalculateDamage(decimal dmg, out decimal actualDmg);

        public abstract bool InRange(CombatObject obj);

        public abstract void Location(out uint x, out uint y);

        public abstract void ReceiveReward(int reward, Resource resource);

        #endregion

        #region IComparable<object> Members

        public virtual int CompareTo(object other)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region Persistable Members

        public bool DbPersisted { get; set; }

        public abstract string DbTable { get; }

        public abstract DbColumn[] DbPrimaryKey { get; }

        public abstract IEnumerable<DbDependency> DbDependencies { get; }

        public abstract DbColumn[] DbColumns { get; }

        #endregion

        #region Public Methods
        public virtual void CleanUp()
        {
            Disposed = true;

            DbPersistance.Current.Delete(this);
        }

        public bool CanSee(CombatObject obj, uint lowestSteath)
        {
            return Visibility >= obj.Stats.Stl || lowestSteath >= obj.Stats.Stl;
        }

        public void ParticipatedInRound()
        {
            LastRound++;
            RoundsParticipated++;
        }
        #endregion

        public int Hash
        {
            get
            {
                return City.Hash;
            }
        }

        public object Lock
        {
            get
            {
                return City.Lock;
            }
        }
    }
}