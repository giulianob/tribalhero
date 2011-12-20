#region

using System;
using Game.Data;
using Game.Data.Stats;
using Game.Setup;
using Ninject;
using Persistance;

#endregion

namespace Game.Battle
{
    public enum BattleClass : byte
    {
        Structure = 0,
        Unit = 1
    }

    public abstract class CombatObject : IComparable<object>, IPersistableObject
    {
        protected IBattleManager battleManager;

        protected CombatObject()
        {
            MinDmgDealt = ushort.MaxValue;
            MinDmgRecv = ushort.MaxValue;
        }

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

        public bool Disposed { get; set; }

        public IBattleManager Battle
        {
            get
            {
                return battleManager;
            }
        }

        public virtual bool IsDead
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual BaseBattleStats BaseStats
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
            set
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual BattleStats Stats
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
            set
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual ushort Type
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual BattleClass ClassType
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual ushort Count
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual decimal Hp
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual uint Visibility
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual uint PlayerId
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual City City
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual byte Lvl
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        public virtual short Stamina
        {
            get
            {
                throw new Exception("NOT IMPLEMENTED");
            }
        }

        #region IComparable<object> Members

        public virtual int CompareTo(object other)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        public virtual string DbTable
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public virtual DbColumn[] DbPrimaryKey
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public virtual DbDependency[] DbDependencies
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public virtual DbColumn[] DbColumns
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion

        public virtual void CleanUp()
        {
            Disposed = true;

            Ioc.Kernel.Get<IDbManager>().Delete(this);
        }

        public virtual void ExitBattle()
        {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
        {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void CalculateDamage(decimal dmg, out decimal actualDmg)
        {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual bool InRange(CombatObject obj)
        {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void Location(out uint x, out uint y) {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void ReceiveReward(int reward, Resource resource)
        {
            throw new Exception("NOT IMPLEMENTED");
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

        public void Print()
        {
            //  throw new NotImplementedException();
        }
    }
}