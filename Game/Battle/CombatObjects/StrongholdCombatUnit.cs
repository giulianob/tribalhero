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
using Game.Logic.Formulas;

#endregion

namespace Game.Battle.CombatObjects
{
    public class StrongholdCombatUnit : CombatObject
    {
        public const string DB_TABLE = "stronghold_combat_units";

        private readonly byte lvl;

        private readonly BattleStats stats;

        private readonly ushort type;

        private readonly UnitFactory unitFactory;

        private readonly Formula formula;

        private ushort count;

        public StrongholdCombatUnit(uint id,
                                    uint battleId,
                                    ushort type,
                                    byte lvl,
                                    ushort count,
                                    IStronghold stronghold,
                                    UnitFactory unitFactory,
                                    IBattleFormulas battleFormulas,
                                    Formula formula)
                : base(id, battleId, battleFormulas)
        {
            Stronghold = stronghold;
            this.type = type;
            this.count = count;
            this.unitFactory = unitFactory;
            this.lvl = lvl;
            this.formula = formula;

            stats = new BattleStats(unitFactory.GetUnitStats(type, lvl).Battle);
            LeftOverHp = stats.MaxHp;
        }

        public StrongholdCombatUnit(uint id,
                                    uint battleId,
                                    ushort type,
                                    byte lvl,
                                    ushort count,
                                    IStronghold stronghold,
                                    decimal leftOverHp,
                                    UnitFactory unitFactory,
                                    IBattleFormulas battleFormulas,
                                    Formula formula)
                : base(id, battleId, battleFormulas)
        {
            Stronghold = stronghold;
            this.type = type;
            this.count = count;
            this.unitFactory = unitFactory;
            this.lvl = lvl;
            LeftOverHp = leftOverHp;
            this.formula = formula;

            stats = new BattleStats(unitFactory.GetUnitStats(type, lvl).Battle);
        }

        private IStronghold Stronghold { get; set; }

        private decimal LeftOverHp { get; set; }

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

        public override byte Size
        {
            get
            {
                return 1;
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
                return unitFactory.GetUnitStats(type, lvl).Upkeep * count;
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
                return Math.Max(0, stats.MaxHp * (count - 1) + LeftOverHp);
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
                        new DbColumn("group_id", GroupId, DbType.UInt32), new DbColumn("level", lvl, DbType.Byte),
                        new DbColumn("count", count, DbType.UInt16), new DbColumn("type", type, DbType.UInt16),
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
                                                    IBattleRandom random,
                                                    decimal baseDmg,
                                                    int attackIndex,
                                                    out decimal actualDmg)
        {
            // Miss chance
            actualDmg = BattleFormulas.GetDmgWithMissChance(attackers.Upkeep, defenders.Upkeep, baseDmg, random);
        }

        public override void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
        {
            attackPoints = 0;
            returning = null;

            ushort dead = 0;
            if (dmg >= LeftOverHp)
            {
                dmg -= LeftOverHp;
                LeftOverHp = stats.MaxHp;
                dead++;
            }

            dead += (ushort)(dmg / stats.MaxHp);
            LeftOverHp -= dmg % stats.MaxHp;

            if (dead > 0)
            {
                if (dead > count)
                {
                    dead = count;
                }
                // Find out how many points the attacker should get
                attackPoints = formula.GetUnitKilledAttackPoint(type, lvl, dead);

                // Remove troops that died from the count
                count -= dead;
            }


        }

        public override void ReceiveReward(int attackPoint, Resource resource)
        {
        }
    }
}