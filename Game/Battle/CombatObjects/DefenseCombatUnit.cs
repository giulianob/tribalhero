#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Ninject;
using Persistance;

#endregion

namespace Game.Battle.CombatObjects
{
    public class DefenseCombatUnit : CityCombatObject
    {
        public const string DB_TABLE = "defense_combat_units";

        private readonly FormationType formation;

        private readonly byte lvl;

        private readonly BattleStats stats;

        private readonly ITroopStub troopStub;

        private readonly ushort type;

        private ushort count;

        private decimal leftOverHp;

        private readonly Formula formula;

        private byte eachUnitUpkeep;

        public DefenseCombatUnit(uint id,
                                 uint battleId,
                                 ITroopStub stub,
                                 FormationType formation,
                                 ushort type,
                                 byte lvl,
                                 ushort count,
                                 IBattleFormulas battleFormulas,
                                 Formula formula,
                                 UnitFactory unitFactory)
                : base(id, battleId, battleFormulas)
        {
            troopStub = stub;
            this.formation = formation;
            this.type = type;
            this.count = count;
            this.formula = formula;
            this.lvl = lvl;

            stats = stub.Template[type];
            leftOverHp = stats.MaxHp;

            eachUnitUpkeep = unitFactory.GetUnitStats(type, lvl).Upkeep;
        }

        public DefenseCombatUnit(uint id,
                                 uint battleId,
                                 ITroopStub stub,
                                 FormationType formation,
                                 ushort type,
                                 byte lvl,
                                 ushort count,
                                 decimal leftOverHp,
                                 IBattleFormulas battleFormulas,
                                 Formula formula,
				 UnitFactory unitFactory)
                : this(id, battleId, stub, formation, type, lvl, count, battleFormulas, formula, unitFactory)
        {
            this.leftOverHp = leftOverHp;
        }

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

        public override BattleStats Stats
        {
            get
            {
                return stats;
            }
        }

        public override int Upkeep
        {
            get
            {
                return eachUnitUpkeep * count;
            }
        }

        public virtual short Stamina
        {
            get
            {
                return -1;
            }
        }

        public override uint Visibility
        {
            get
            {
                return Stats.Rng;
            }
        }

        public override byte Size
        {
            get
            {
                return 1;
            }
        }

        public override ICity City
        {
            get
            {
                return TroopStub.City;
            }
        }

        public override byte Lvl
        {
            get
            {
                return lvl;
            }
        }

        public override decimal Hp
        {
            get
            {
                return Math.Max(0, stats.MaxHp * (count - 1) + leftOverHp);
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
                        new DbColumn("last_round", LastRound, DbType.UInt32),
                        new DbColumn("rounds_participated", RoundsParticipated, DbType.UInt32),
                        new DbColumn("group_id", GroupId, DbType.UInt32),
                        new DbColumn("formation_type", (byte)formation, DbType.Byte),
                        new DbColumn("level", lvl, DbType.Byte), new DbColumn("count", count, DbType.UInt16),
                        new DbColumn("type", type, DbType.UInt16),
                        new DbColumn("troop_stub_city_id", TroopStub.City.Id, DbType.UInt32),
                        new DbColumn("troop_stub_id", TroopStub.TroopId, DbType.UInt16),
                        new DbColumn("left_over_hp", leftOverHp, DbType.Decimal),
                        new DbColumn("damage_min_dealt", MinDmgDealt, DbType.UInt16),
                        new DbColumn("damage_max_dealt", MaxDmgDealt, DbType.UInt16),
                        new DbColumn("damage_min_received", MinDmgRecv, DbType.UInt16),
                        new DbColumn("damage_max_received", MaxDmgRecv, DbType.UInt16),
                        new DbColumn("damage_dealt", DmgDealt, DbType.Decimal),
                        new DbColumn("damage_received", DmgRecv, DbType.Decimal),
                        new DbColumn("hits_dealt", HitDealt, DbType.UInt16),
                        new DbColumn("hits_dealt_by_unit", HitDealtByUnit, DbType.UInt32),
                        new DbColumn("hits_received", HitRecv, DbType.UInt16)
                };
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
            if (TroopStub.Station != null)
            {
                return troopStub.Station.PrimaryPosition;
            }

            return City.PrimaryPosition;
        }

        public override byte AttackRadius()
        {
            return byte.MaxValue;
        }

        public override void ReceiveReward(int reward, Resource resource)
        {
            throw new Exception("Why is a defense combat unit receiving rewards dammit?");
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

            // Splash dmg reduction
            actualDmg = BattleFormulas.SplashReduction(this, actualDmg, attackIndex);
            
            // if hp is less than 20% of the original total HP(entire group), lastStand kicks in.
            if (Hp < (Hp + DmgRecv) / 5)
            {
                var percent = TroopStub.City.Technologies
                                       .GetEffects(EffectCode.LastStand).Where(tech => BattleFormulas.UnitStatModCheck(Stats.Base,
                                                                                                                       TroopBattleGroup.Attack,
                                                                                                                       (string)tech.Value[1]))
                                       .DefaultIfEmpty()
                                       .Max(x => x == null ? 0 : (int)x.Value[0]);

                actualDmg = actualDmg * (100 - percent) / 100;
            }
        }

        public override void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
        {
            attackPoints = 0;

            ushort dead = 0;
            if (dmg >= leftOverHp)
            {
                dmg -= leftOverHp;
                leftOverHp = stats.MaxHp;
                dead++;
            }

            dead += (ushort)(dmg / stats.MaxHp);
            leftOverHp -= dmg % stats.MaxHp;

            if (dead > 0)
            {
                if (dead > count)
                {
                    dead = count;
                }

                count -= dead;

                attackPoints = formula.GetUnitKilledAttackPoint(type, lvl, dead);

                // Remove dead units from troop stub
                TroopStub.BeginUpdate();
                TroopStub[formation].Remove(type, dead);
                TroopStub.EndUpdate();
            }

            returning = null;
        }

        #region ICombatUnit Members

        public override ITroopStub TroopStub
        {
            get
            {
                return troopStub;
            }
        }

        public FormationType Formation
        {
            get
            {
                return formation;
            }
        }

        public override Resource Loot
        {
            get
            {
                return new Resource();
            }
        }

        #endregion
    }
}