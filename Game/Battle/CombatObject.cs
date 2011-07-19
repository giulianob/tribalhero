#region

using System;
using Game.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Setup;

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
        protected BattleManager battleManager;

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

        public BattleManager Battle
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

        public virtual uint Hp
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

            Global.DbManager.Delete(this);
        }

        public virtual void ExitBattle()
        {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void TakeDamage(int dmg, out Resource returning, out int attackPoints)
        {
            throw new Exception("NOT IMPLEMENTED");
        }

        public virtual void CalculateDamage(ushort dmg, out ushort actualDmg)
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

        public bool CanSee(CombatObject obj)
        {
            // In order to implement visibility as discussed in http://trac.tribalhero.com/wiki/Discussion%2011/08/10
            // we always take the lowest RoundsParticipated of the two objects.

            // Removed the chance to hit even when total is lower stl, making it more deterministic
            int minRoundsParticipated = Math.Min(RoundsParticipated, obj.RoundsParticipated);
            var totalVision = (int)(Visibility + minRoundsParticipated);
            switch(obj.BaseStats.Weapon)
            {
                case WeaponType.Bow:
                    totalVision += Math.Max((minRoundsParticipated * (BaseStats.Spd-5) / 15),0);
                    break;
                default:
                    break;
            }

            if (totalVision >= obj.Stats.Stl)
            {
#if DEBUG
                Global.Logger.Debug(string.Format("Total Vision[{2}] CanSee [{0}] Stl[{1}]",
                                                        obj.ClassType == BattleClass.Unit ? UnitFactory.GetName(obj.Type, 1) : StructureFactory.GetName(obj.Type, 1),
                                                        obj.Stats.Stl,
                                                        totalVision));
#endif
                return true;
            }

            return false;
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