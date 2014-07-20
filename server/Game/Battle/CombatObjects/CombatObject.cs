#region

using System;
using System.Collections.Generic;
using Game.Comm;
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
        protected readonly IBattleFormulas BattleFormulas;

        private readonly IDbManager dbManager;

        protected readonly uint BattleId;

        protected CombatObject(uint id, uint battleId, IBattleFormulas battleFormulas, IDbManager dbManager)
        {
            Id = id;
            MinDmgDealt = ushort.MaxValue;
            MinDmgRecv = ushort.MaxValue;
            BattleId = battleId;
            BattleFormulas = battleFormulas;
            this.dbManager = dbManager;

            IsWaitingToJoinBattle = true;
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

        public bool IsWaitingToJoinBattle { get; set; }

        #endregion

        #region Abstract Properties

        public abstract byte Size { get; }

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

        public virtual decimal AttackBonus(ICombatObject target)
        {
            return 0;
        }

        public virtual decimal DefenseBonus(ICombatObject attacker)
        {
            return 0;
        }

        public abstract void TakeDamage(decimal dmg, out Resource returning, out int attackPoints);

        public abstract void CalcActualDmgToBeTaken(ICombatList attackers,
                                                    ICombatList defenders,
                                                    IBattleRandom random,
                                                    decimal baseDmg,
                                                    int attackIndex,
                                                    out decimal actualDmg);

        public abstract bool InRange(ICombatObject obj);

        public abstract Position Location();

        public abstract byte AttackRadius();

        public abstract void ReceiveReward(int attackPoints, Resource resource);

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

        public void JoinBattle(uint round)
        {
            IsWaitingToJoinBattle = false;
            LastRound = round;
        }

        public virtual void ExitBattle()
        {
            Disposed = true;
            dbManager.Delete(this);
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

        public bool HasAttacked(uint round)
        {
            return LastRound > round;
        }

        public virtual void AddPacketInfo(Packet packet)
        {
            packet.AddUInt32(Id);
            packet.AddByte((byte)ClassType);
            packet.AddUInt16(Type);
            packet.AddByte(Lvl);
            packet.AddFloat((float)Hp);
            packet.AddFloat((float)Stats.MaxHp);
            packet.AddUInt16(Count);
        }

        #endregion
    }
}