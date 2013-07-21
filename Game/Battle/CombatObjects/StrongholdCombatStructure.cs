#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Stronghold;
using Game.Map;
using Game.Setup;
using Persistance;

#endregion

namespace Game.Battle.CombatObjects
{
    /// <summary>
    ///     NOTE: This class cant be used directly since the DbLoader is always creating a StrongholdCombatGate when loading from this table
    ///     If we need to use this class then make the DbLoader load either a StrongholdCombatStructure or StrongholdCombatGate depending on what was created.
    /// </summary>
    public abstract class StrongholdCombatStructure : CombatObject
    {
        public const string DB_TABLE = "stronghold_combat_structures";

        private readonly byte lvl;

        private readonly BattleStats stats;

        private readonly ushort type;

        protected decimal hp;

        private IStructureBaseStats structureBaseStats;

        protected StrongholdCombatStructure(uint id,
                                         uint battleId,
                                         ushort type,
                                         byte lvl,
                                         decimal hp,
                                         IStronghold stronghold,
                                         IStructureCsvFactory structureCsvFactory,
                                         IBattleFormulas battleFormulas)
                : base(id, battleId, battleFormulas)
        {
            Stronghold = stronghold;
            this.type = type;
            this.lvl = lvl;
            this.hp = hp;

            structureBaseStats = structureCsvFactory.GetBaseStats(type, lvl);
            stats = new BattleStats(structureBaseStats.Battle);
        }

        protected IStronghold Stronghold { get; private set; }

        public override BattleClass ClassType
        {
            get
            {
                return BattleClass.Structure;
            }
        }

        public override byte Size
        {
            get
            {
                return structureBaseStats.Size;
            }
        }

        public override bool IsDead
        {
            get
            {
                return Hp == 0;
            }
        }

        public override ushort Count
        {
            get
            {
                return (ushort)(Hp > 0 ? 1 : 0);                
            }
        }

        public override ushort Type
        {
            get
            {
                return type;
            }
        }

        public override int Upkeep
        {
            get
            {
                return BattleFormulas.GetUnitsPerStructure(lvl) / 5;
            }
        }

        public override BattleStats Stats
        {
            get
            {
                return stats;
            }
        }

        public override uint Visibility
        {
            get
            {
                return Stats.Rng;
            }
        }

        public override byte Lvl
        {
            get
            {
                return lvl;
            }
        }

        public override int Hash
        {
            get
            {
                return Stronghold.Hash;
            }
        }

        public override object Lock
        {
            get
            {
                return Stronghold.Lock;
            }
        }

        public override decimal Hp
        {
            get
            {
                return hp;
            }
        }

        public override string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public override DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("battle_id", BattleId, DbType.UInt32), new DbColumn("id", Id, DbType.UInt32)};
            }
        }

        public override IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public override DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("stronghold_id", Stronghold.ObjectId, DbType.UInt32),
                        new DbColumn("group_id", GroupId, DbType.UInt32), 
                        new DbColumn("level", lvl, DbType.Byte),
                        new DbColumn("type", type, DbType.UInt16), 
                        new DbColumn("hp", hp, DbType.Decimal),
                        new DbColumn("last_round", LastRound, DbType.UInt32),
                        new DbColumn("rounds_participated", RoundsParticipated, DbType.UInt32),
                        new DbColumn("damage_min_dealt", MinDmgDealt, DbType.UInt16),
                        new DbColumn("damage_max_dealt", MaxDmgDealt, DbType.UInt16),
                        new DbColumn("damage_min_received", MinDmgRecv, DbType.UInt16),
                        new DbColumn("damage_max_received", MaxDmgRecv, DbType.UInt16),
                        new DbColumn("damage_dealt", DmgDealt, DbType.Int32),
                        new DbColumn("damage_received", DmgRecv, DbType.Int32),
                        new DbColumn("hits_dealt", HitDealt, DbType.UInt16),
                        new DbColumn("hits_dealt_by_unit", HitDealtByUnit, DbType.UInt32),
                        new DbColumn("hits_received", HitRecv, DbType.UInt16),
                };
            }
        }

        public override Resource Loot
        {
            get
            {
                return new Resource();
            }
        }

        public override int LootPerRound()
        {
            return 0;
        }

        public override bool InRange(ICombatObject obj)
        {
            return true;
        }

        public override Position Location()
        {
            return Stronghold.PrimaryPosition;
        }

        public override byte AttackRadius()
        {
            return byte.MaxValue;
        }

        public override void CalcActualDmgToBeTaken(ICombatList attackers,
                                                    ICombatList defenders,
                                                    decimal baseDmg,
                                                    int attackIndex,
                                                    out decimal actualDmg)
        {
            // Miss chance
            actualDmg = BattleFormulas.GetDmgWithMissChance(attackers.Upkeep, defenders.Upkeep, baseDmg);
        }

        public override void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
        {
            attackPoints = 0;
            returning = null;

            hp = Math.Max(0, hp - dmg);
        }

        public override void ReceiveReward(int attackPoint, Resource resource)
        {
        }

        public override int CompareTo(object other)
        {
            if (other is StrongholdCombatStructure)
            {
                return other == this ? 0 : 1;
            }

            return -1;
        }
    }
}