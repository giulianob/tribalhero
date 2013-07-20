#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Map;
using Persistance;

#endregion

namespace Game.Battle.CombatObjects
{
    public abstract class CombatObject : ICombatObject
    {
        protected readonly BattleFormulas BattleFormulas;

        protected readonly uint BattleId;

        /// <summary>
        ///     Parameterless constructor for mocking only
        /// </summary>
        [Obsolete("Used for testing only", true)]
        protected CombatObject()
        {
        }

        protected CombatObject(uint id, uint battleId, BattleFormulas battleFormulas)
        {
            Id = id;
            MinDmgDealt = ushort.MaxValue;
            MinDmgRecv = ushort.MaxValue;
            BattleId = battleId;
            BattleFormulas = battleFormulas;
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

        public int RoundsParticipated { get; set; }

        public uint LastRound { get; set; }

        public uint Id { get; private set; }

        public uint GroupId { get; set; }

        public bool Disposed { get; private set; }

        #endregion

        #region Abstract Properties

        public abstract int Upkeep { get; }

        public abstract bool IsDead { get; }

        public abstract BattleStats Stats { get; }

        public abstract ushort Type { get; }

        public abstract Resource Loot { get; }

        public abstract BattleClass ClassType { get; }

        public abstract ushort Count { get; }

        public abstract decimal Hp { get; }

        public abstract uint Visibility { get; }

        public abstract byte Lvl { get; }

        public abstract int Hash { get; }

        public abstract object Lock { get; }

        #endregion

        #region Abstract Methods        

        public abstract void TakeDamage(decimal dmg, out Resource returning, out int attackPoints);

        public abstract void CalcActualDmgToBeTaken(ICombatList attackers,
                                                    ICombatList defenders,
                                                    decimal baseDmg,
                                                    int attackIndex,
                                                    out decimal actualDmg);

        public abstract bool InRange(ICombatObject obj);

        public abstract Position Location();

        public abstract byte AttackRadius();

        public abstract void ReceiveReward(int reward, Resource resource);

        public abstract int LootPerRound();

        #endregion

        #region IComparable<object> Members

        #endregion

        #region Persistable Members

        public bool DbPersisted { get; set; }

        public abstract string DbTable { get; }

        public abstract DbColumn[] DbPrimaryKey { get; }

        public abstract IEnumerable<DbDependency> DbDependencies { get; }

        public abstract DbColumn[] DbColumns { get; }

        #endregion

        #region Public Methods

        public virtual void ExitBattle()
        {
            Disposed = true;
            DbPersistance.Current.Delete(this);
        }

        public bool CanSee(ICombatObject obj, uint lowestSteath)
        {
            return Visibility >= obj.Stats.Stl || lowestSteath >= obj.Stats.Stl;
        }

        public void ParticipatedInRound(uint round)
        {
            LastRound = round + 1;
            RoundsParticipated++;
        }

        #endregion
    }
}