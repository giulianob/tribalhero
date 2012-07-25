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
using Persistance;

#endregion

namespace Game.Battle.CombatObjects
{
    public class AttackCombatUnit : CityCombatObject, ICombatUnit
    {
        public const string DB_TABLE = "attack_combat_units";

        private readonly FormationType formation;

        private readonly Resource loot = new Resource();

        private readonly byte lvl;

        private readonly BattleStats stats;

        private readonly ushort type;

        private readonly ITroopObject troopObject;

        private ushort count;

        private readonly UnitFactory unitFactory;

        public AttackCombatUnit(uint id,
                                uint battleId,
                                ITroopObject troopObject,
                                FormationType formation,
                                ushort type,
                                byte lvl,
                                ushort count,
                                UnitFactory unitFactory,
                                BattleFormulas battleFormulas)
                : base(id, battleId, battleFormulas)
        {
            this.troopObject = troopObject;
            this.formation = formation;
            this.type = type;
            this.count = count;
            this.unitFactory = unitFactory;
            this.lvl = lvl;

            stats = troopObject.Stub.Template[type];
            LeftOverHp = stats.MaxHp;
        }

        /// <summary>
        /// DB loader constructor
        /// </summary>
        public AttackCombatUnit(uint id,
                                uint battleId,
                                ITroopObject troopObject,
                                FormationType formation,
                                ushort type,
                                byte lvl,
                                ushort count,
                                decimal leftOverHp,
                                Resource loot,
                                UnitFactory unitFactory,
                                BattleFormulas battleFormulas)
                : this(id, battleId, troopObject, formation, type, lvl, count, unitFactory, battleFormulas)
        {
            LeftOverHp = leftOverHp;

            this.loot = loot;
        }

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
                return unitFactory.GetUnitStats(type, lvl).Upkeep*count;
            }
        }

        public override BattleStats Stats
        {
            get
            {
                return stats;
            }
        }

        public virtual short Stamina
        {
            get
            {
                return troopObject.Stats.Stamina;
            }
        }

        public override uint Visibility
        {
            get
            {
                return Stats.Rng;
            }
        }

        public override uint PlayerId
        {
            get
            {
                return TroopStub.City.Owner.PlayerId;
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
                return Math.Max(0, stats.MaxHp*(count - 1) + LeftOverHp);
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
                return new[] { new DbColumn("battle_id", BattleId, DbType.UInt32), new DbColumn("id", Id, DbType.UInt32)};
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
                        new DbColumn("group_id", GroupId, DbType.UInt32), new DbColumn("formation_type", (byte)formation, DbType.Byte),
                        new DbColumn("level", lvl, DbType.Byte), new DbColumn("count", count, DbType.UInt16), new DbColumn("type", type, DbType.UInt16),
                        new DbColumn("left_over_hp", LeftOverHp, DbType.Decimal), new DbColumn("last_round", LastRound, DbType.UInt32),
                        new DbColumn("rounds_participated", RoundsParticipated, DbType.UInt32), new DbColumn("troop_stub_city_id", TroopStub.City.Id, DbType.UInt32),
                        new DbColumn("troop_object_id", troopObject.ObjectId, DbType.UInt32), new DbColumn("damage_min_dealt", MinDmgDealt, DbType.UInt16),
                        new DbColumn("damage_max_dealt", MaxDmgDealt, DbType.UInt16), new DbColumn("damage_min_received", MinDmgRecv, DbType.UInt16),
                        new DbColumn("damage_max_received", MaxDmgRecv, DbType.UInt16), new DbColumn("damage_dealt", DmgDealt, DbType.Int32),
                        new DbColumn("damage_received", DmgRecv, DbType.Int32), new DbColumn("hits_dealt", HitDealt, DbType.UInt16),
                        new DbColumn("hits_dealt_by_unit", HitDealtByUnit, DbType.UInt32), new DbColumn("hits_received", HitRecv, DbType.UInt16),
                        new DbColumn("loot_crop", loot.Crop, DbType.UInt32), new DbColumn("loot_wood", loot.Wood, DbType.UInt32),
                        new DbColumn("loot_iron", loot.Iron, DbType.UInt32), new DbColumn("loot_gold", loot.Gold, DbType.UInt32),
                        new DbColumn("loot_labor", loot.Labor, DbType.UInt32)
                };
            }
        }

        public override int LootPerRound()
        {
            return BattleFormulas.GetLootPerRoundForCity(City);
        }

        #region ICombatUnit Members

        public override ITroopStub TroopStub
        {
            get
            {
                return troopObject.Stub;
            }
        }

        public override Resource Loot
        {
            get
            {
                return loot;
            }
        }

        public FormationType Formation
        {
            get
            {
                return formation;
            }
        }

        public bool IsAttacker
        {
            get
            {
                return true;
            }
        }

        #endregion

        public override bool InRange(CombatObject obj)
        {
            if (obj is AttackCombatUnit || obj is DefenseCombatUnit) //all units can attack other units
            {
                return true;
            }

            if (obj is CombatStructure)
            {
                return RadiusLocator.Current.IsOverlapping(Location(), AttackRadius(), obj.Location(), obj.AttackRadius());
            }

            throw new Exception(string.Format("Why is an attack combat unit trying to kill a unit of type {0}?", obj.GetType().FullName));
        }

        public override Location Location()
        {
            return new Location(troopObject.X, troopObject.Y);
        }

        public override byte AttackRadius()
        {
            return troopObject.Stats.AttackRadius;
        }

        public override void CalcActualDmgToBeTaken(ICombatList attackers, ICombatList defenders, decimal baseDmg, int attackIndex, out decimal actualDmg)
        {
            // Miss chance
            actualDmg = BattleFormulas.GetDmgWithMissChance(attackers.Upkeep, defenders.Upkeep, baseDmg);

            // Splash dmg reduction
            actualDmg = BattleFormulas.SplashReduction(this, actualDmg, attackIndex);

            // if hp is less than 20% of the original total HP(entire group), lastStand kicks in.
            if (Hp < (Hp + DmgRecv)/5m)
            {
                var lastStandEffects = TroopStub.City.Technologies.GetEffects(EffectCode.LastStand);
                var percent =
                        lastStandEffects.Where(tech => BattleFormulas.UnitStatModCheck(Stats.Base, TroopBattleGroup.Attack, (string)tech.Value[1])).
                                DefaultIfEmpty().Max(x => x == null ? 0 : (int)x.Value[0]);

                actualDmg = actualDmg*(100 - percent)/100m;
            }
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

            dead += (ushort)(dmg/stats.MaxHp);
            LeftOverHp -= dmg%stats.MaxHp;

            if (dead > 0)
            {
                if (dead > count)
                {
                    dead = count;
                }

                // Find out how many points the defender should get
                attackPoints = Formula.Current.GetUnitKilledAttackPoint(type, lvl, dead);

                // Remove troops that died from the count
                count -= dead;

                // Remove dead troops from the troop stub 
                TroopStub.BeginUpdate();
                TroopStub[formation].Remove(type, dead);
                TroopStub.EndUpdate();

                // Figure out how much loot we have to return to the city
                int totalCarry = Stats.Carry*Count;
                returning = new Resource(loot.Crop > totalCarry/Config.resource_crop_ratio ? loot.Crop - totalCarry/Config.resource_crop_ratio : 0,
                                         loot.Gold > totalCarry/Config.resource_gold_ratio ? loot.Gold - totalCarry/Config.resource_gold_ratio : 0,
                                         loot.Iron > totalCarry/Config.resource_iron_ratio ? loot.Iron - totalCarry/Config.resource_iron_ratio : 0,
                                         loot.Wood > totalCarry/Config.resource_wood_ratio ? loot.Wood - totalCarry/Config.resource_wood_ratio : 0,
                                         loot.Labor > totalCarry/Config.resource_labor_ratio ? loot.Wood - totalCarry/Config.resource_labor_ratio : 0);

                // Remove it from our loot
                loot.Subtract(returning);

                // Since the loot is stored at the troop stub as well, we need to remove it from there too
                troopObject.BeginUpdate();
                troopObject.Stats.Loot.Subtract(returning);
                troopObject.EndUpdate();
            }
        }

        public override void ExitBattle()
        {
        }

        public override void ReceiveReward(int attackPoint, Resource resource)
        {
            loot.Add(resource);

            troopObject.BeginUpdate();
            troopObject.Stats.AttackPoint += attackPoint;
            troopObject.Stats.Loot.Add(resource);
            troopObject.EndUpdate();
        }

        public override int CompareTo(object other)
        {
            if (other is ITroopStub)
            {
                return other == TroopStub ? 0 : 1;
            }

            return -1;
        }
    }
}