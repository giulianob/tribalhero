#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stats;
using Game.Map;
using Game.Setup;
using Persistance;
using Game.Logic.Formulas;

#endregion

namespace Game.Battle.CombatObjects
{
    public class BarbarianTribeCombatUnit : CombatObject
    {
        public const string DB_TABLE = "barbarian_tribe_combat_units";

        private readonly byte lvl;

        private readonly BaseUnitStats baseStats;

        private readonly BattleStats battleStats;

        private readonly ushort type;

        private readonly Formula formula;

        private ushort count;

        public BarbarianTribeCombatUnit(uint id,
                                    uint battleId,
                                    ushort type,
                                    byte lvl,
                                    ushort count,
                                    BaseUnitStats unitBaseStats,
                                    IBarbarianTribe barbarianTribe,                                    
                                    BattleFormulas battleFormulas,
                                    Formula formula)
                : base(id, battleId, battleFormulas)
        {
            BarbarianTribe = barbarianTribe;
            this.type = type;
            this.count = count;            
            this.lvl = lvl;
            this.formula = formula;

            baseStats = unitBaseStats;
            battleStats = new BattleStats(unitBaseStats.Battle);

            LeftOverHp = baseStats.Battle.MaxHp;
        }

        private IBarbarianTribe BarbarianTribe { get; set; }

        public decimal LeftOverHp { get; set; }

        public override BattleClass ClassType
        {
            get
            {
                return BattleClass.Unit;
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
                return count;
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
                return baseStats.Upkeep * count;
            }
        }

        public override BattleStats Stats
        {
            get
            {
                return battleStats;
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
                return BarbarianTribe.Hash;
            }
        }

        public override object Lock
        {
            get
            {
                return BarbarianTribe.Lock;
            }
        }

        public override decimal Hp
        {
            get
            {
                return Math.Max(0, battleStats.MaxHp * (count - 1) + LeftOverHp);
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
                        new DbColumn("barbarian_tribe_id", BarbarianTribe.Id, DbType.UInt32),
                        new DbColumn("group_id", GroupId, DbType.UInt32), 
                        new DbColumn("level", lvl, DbType.Byte),
                        new DbColumn("count", count, DbType.UInt16), 
                        new DbColumn("type", type, DbType.UInt16),
                        new DbColumn("left_over_hp", LeftOverHp, DbType.Decimal),
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

        public bool IsAttacker
        {
            get
            {
                return false;
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
            return new Position(BarbarianTribe.X, BarbarianTribe.Y);
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

            ushort dead = 0;
            if (dmg >= LeftOverHp)
            {
                dmg -= LeftOverHp;
                LeftOverHp = battleStats.MaxHp;
                dead++;
            }

            dead += (ushort)(dmg / battleStats.MaxHp);
            LeftOverHp -= dmg % battleStats.MaxHp;

            if (dead > 0)
            {
                if (dead > count)
                {
                    dead = count;
                }
                
                attackPoints = formula.GetUnitKilledAttackPoint(type, lvl, dead);

                // Remove troops that died from the count
                count -= dead;
            }
        }

        public override void ReceiveReward(int attackPoint, Resource resource)
        {
        }

        public override int CompareTo(object other)
        {
            if (other is BarbarianTribeCombatUnit)
            {
                return other == this ? 0 : 1;
            }

            return -1;
        }
    }
}