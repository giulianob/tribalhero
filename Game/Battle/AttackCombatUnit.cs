#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Formulas;
using Game.Setup;
using System.Linq;
#endregion

namespace Game.Battle
{
    public class AttackCombatUnit : CombatObject, ICombatUnit
    {
        public const string DB_TABLE = "combat_units";
        private readonly FormationType formation;
        private readonly Resource loot = new Resource();
        private readonly byte lvl;
        private readonly BattleStats stats;
        private readonly ushort type;
        private ushort count;

        public AttackCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, byte lvl, ushort count)
        {
            TroopStub = stub;
            this.formation = formation;
            this.type = type;
            this.count = count;
            battleManager = owner;
            this.lvl = lvl;

            stats = stub.Template[type];
            LeftOverHp = stats.MaxHp;
        }

        // Used by the db loader
        public AttackCombatUnit(BattleManager owner,
                                TroopStub stub,
                                FormationType formation,
                                ushort type,
                                byte lvl,
                                ushort count,
                                ushort leftOverHp,
                                Resource loot) : this(owner, stub, formation, type, lvl, count)
        {
            LeftOverHp = leftOverHp;

            this.loot = loot;
        }

        public ushort LeftOverHp { get; set; }

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
                return UnitFactory.GetUnitStats(type, lvl).Upkeep*count;
            }
        }

        public override BaseBattleStats BaseStats
        {
            get
            {
                return UnitFactory.GetBattleStats(type, lvl);
            }
        }

        public override BattleStats Stats
        {
            get
            {
                return stats;
            }
        }

        public override short Stamina
        {
            get
            {
                return TroopStub.TroopObject.Stats.Stamina;
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

        public override City City
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

        public override uint Hp
        {
            get
            {
                return (uint)(Math.Max(0, stats.MaxHp*(count - 1) + LeftOverHp));
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
                return new[] {new DbColumn("id", Id, DbType.UInt32), new DbColumn("city_id", battleManager.City.Id, DbType.UInt32)};
            }
        }

        public override DbDependency[] DbDependencies
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
                               new DbColumn("is_local", false, DbType.Boolean), new DbColumn("left_over_hp", LeftOverHp, DbType.UInt16),
                               new DbColumn("last_round", LastRound, DbType.UInt32), new DbColumn("rounds_participated", RoundsParticipated, DbType.UInt32),
                               new DbColumn("troop_stub_city_id", TroopStub.City.Id, DbType.UInt32), new DbColumn("troop_stub_id", TroopStub.TroopId, DbType.Byte),
                               new DbColumn("damage_min_dealt", MinDmgDealt, DbType.UInt16), new DbColumn("damage_max_dealt", MaxDmgDealt, DbType.UInt16),
                               new DbColumn("damage_min_received", MinDmgRecv, DbType.UInt16), new DbColumn("damage_max_received", MaxDmgRecv, DbType.UInt16),
                               new DbColumn("damage_dealt", DmgDealt, DbType.Int32), new DbColumn("damage_received", DmgRecv, DbType.Int32),
                               new DbColumn("hits_dealt", HitDealt, DbType.UInt16), new DbColumn("hits_dealt_by_unit", HitDealtByUnit, DbType.UInt32),
                               new DbColumn("hits_received", HitRecv, DbType.UInt16), new DbColumn("loot_crop", loot.Crop, DbType.UInt32),
                               new DbColumn("loot_wood", loot.Wood, DbType.UInt32), new DbColumn("loot_iron", loot.Iron, DbType.UInt32),
                               new DbColumn("loot_gold", loot.Gold, DbType.UInt32), new DbColumn("loot_labor", loot.Labor, DbType.UInt32),
                       };
            }
        }

        #region ICombatUnit Members

        public TroopStub TroopStub { get; private set; }

        public Resource Loot
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

        #endregion

        public override bool InRange(CombatObject obj)
        {
            if (obj is AttackCombatUnit || obj is DefenseCombatUnit) //all units can attack other units
                return true;

            if (obj is CombatStructure)
            {
                Structure structure = (obj as CombatStructure).Structure;
                return TroopStub.TroopObject.RadiusDistance(structure) <= structure.Stats.Base.Radius + TroopStub.TroopObject.Stats.AttackRadius;
            }

            throw new Exception(string.Format("Why is an attack combat unit trying to kill a unit of type {0}?", obj.GetType().FullName));
        }

        public override void Location(out uint x, out uint y) {
            x = TroopStub.TroopObject.X;
            y = TroopStub.TroopObject.Y;
        }

        public override void CalculateDamage(ushort dmg, out ushort actualDmg)
        {

            actualDmg = (ushort)Math.Min(Hp, dmg);

            if (this.stats.MaxHp/5 <= Hp) // if hp is less than 20% of max, lastStand kicks in.
                return;

            int percent = TroopStub.City.Technologies.GetEffects(EffectCode.LastStand, EffectInheritance.All).Where(tech => BattleFormulas.UnitStatModCheck(this.BaseStats, TroopBattleGroup.Attack, tech.Value[1], tech.Value[2])).DefaultIfEmpty().Max(x => x == null ? 0 : (int)x.Value[0]);
            if( BattleFormulas.IsAttackMissed((byte)percent) )
            {
                actualDmg = 1;
            }
        }

        public override void TakeDamage(int dmg, out Resource returning, out int attackPoints)
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
            LeftOverHp -= (ushort)(dmg%stats.MaxHp);

            if (dead > 0)
            {
                if (dead > count)
                    dead = count;

                // Find out how many points the defender should get
                attackPoints = Formula.GetUnitKilledAttackPoint(type, lvl, dead);

                // Remove troops that died from the count
                count -= dead;

                // Remove dead troops from the troop stub 
                TroopStub.BeginUpdate();
                TroopStub[formation].Remove(type, dead);
                TroopStub.EndUpdate();

                // Figure out how much loot we have to return to the city
                int totalCarry = BaseStats.Carry*Count;
                returning = new Resource(loot.Crop > totalCarry / Config.resource_crop_ratio ? loot.Crop - totalCarry / Config.resource_crop_ratio : 0,
                                         loot.Gold > totalCarry / Config.resource_gold_ratio ? loot.Gold - totalCarry / Config.resource_gold_ratio : 0,
                                         loot.Iron > totalCarry / Config.resource_iron_ratio ? loot.Iron - totalCarry / Config.resource_iron_ratio : 0,
                                         loot.Wood > totalCarry / Config.resource_wood_ratio ? loot.Wood - totalCarry / Config.resource_wood_ratio : 0,
                                         loot.Labor > totalCarry / Config.resource_labor_ratio ? loot.Wood - totalCarry / Config.resource_labor_ratio : 0);

                // Remove it from our loot
                loot.Subtract(returning);

                // Since the loot is stored at the troop stub as well, we need to remove it from there too
                TroopStub.TroopObject.BeginUpdate();
                TroopStub.TroopObject.Stats.Loot.Subtract(returning);
                TroopStub.TroopObject.EndUpdate();
            }
        }

        public override void CleanUp()
        {
            Global.DbManager.Delete(this);
        }

        public override void ExitBattle()
        {
        }

        public override void ReceiveReward(int attackPoint, Resource resource)
        {
            loot.Add(resource);

            TroopStub.TroopObject.BeginUpdate();
            TroopStub.TroopObject.Stats.AttackPoint += attackPoint;
            TroopStub.TroopObject.Stats.Loot.Add(resource);
            TroopStub.TroopObject.EndUpdate();
        }

        public override int CompareTo(object other)
        {
            if (other is TroopStub)
                return other == TroopStub ? 0 : 1;

            return -1;
        }
    }
}