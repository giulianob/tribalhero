#region

using System;
using System.Data;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;
using Ninject;
using Persistance;

#endregion

namespace Game.Battle
{
    public class DefenseCombatUnit : CombatObject, ICombatUnit
    {
        public const string DB_TABLE = "combat_units";
        private readonly FormationType formation;

        private readonly byte lvl;
        private readonly BattleStats stats;
        private readonly ushort type;
        private ushort count;
        private readonly ITroopStub troopStub;

        public DefenseCombatUnit(IBattleManager owner, ITroopStub stub, FormationType formation, ushort type, byte lvl, ushort count)
        {
            troopStub = stub;
            this.formation = formation;
            this.type = type;
            this.count = count;
            battleManager = owner;
            this.lvl = lvl;

            stats = stub.Template[type];
            LeftOverHp = stats.MaxHp;
        }

        public DefenseCombatUnit(IBattleManager owner, ITroopStub stub, FormationType formation, ushort type, byte lvl, ushort count, decimal leftOverHp)
                : this(owner, stub, formation, type, lvl, count)
        {
            LeftOverHp = leftOverHp;
        }

        public decimal LeftOverHp { set; get; }

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
                return Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type, lvl).Upkeep * count;
            }
        }

        public override short Stamina
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
                return new[] { new DbColumn("id", Id, DbType.UInt32), new DbColumn("city_id", battleManager.City.Id, DbType.UInt32) };
            }
        }

        public override DbDependency[] DbDependencies
        {
            get
            {
                return new DbDependency[] { };
            }
        }

        public override DbColumn[] DbColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("last_round", LastRound, DbType.UInt32), new DbColumn("rounds_participated", RoundsParticipated, DbType.UInt32),
                               new DbColumn("group_id", GroupId, DbType.UInt32), new DbColumn("formation_type", (byte)formation, DbType.Byte),
                               new DbColumn("level", lvl, DbType.Byte), new DbColumn("count", count, DbType.UInt16), new DbColumn("type", type, DbType.UInt16),
                               new DbColumn("is_local", true, DbType.Boolean), new DbColumn("troop_stub_city_id", TroopStub.City.Id, DbType.UInt32),
                               new DbColumn("troop_stub_id", TroopStub.TroopId, DbType.Byte), new DbColumn("left_over_hp", LeftOverHp, DbType.Decimal),
                               new DbColumn("damage_min_dealt", MinDmgDealt, DbType.UInt16), new DbColumn("damage_max_dealt", MaxDmgDealt, DbType.UInt16),
                               new DbColumn("damage_min_received", MinDmgRecv, DbType.UInt16), new DbColumn("damage_max_received", MaxDmgRecv, DbType.UInt16),
                               new DbColumn("damage_dealt", DmgDealt, DbType.Decimal), new DbColumn("damage_received", DmgRecv, DbType.Decimal),
                               new DbColumn("hits_dealt", HitDealt, DbType.UInt16), new DbColumn("hits_dealt_by_unit", HitDealtByUnit, DbType.UInt32),
                               new DbColumn("hits_received", HitRecv, DbType.UInt16),
                       };
            }
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

        public bool IsAttacker
        {
            get
            {
                return false;
            }
        }

        public override Resource GroupLoot
        {
            get
            {
                return new Resource();
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

        public override bool InRange(CombatObject obj)
        {
            return true;
        }

        public override void Location(out uint x, out uint y)
        {
            if (TroopStub.TroopObject == null)
            {
                x = City.X;
                y = City.Y;
            }
            else
            {
                x = TroopStub.TroopObject.X;
                y = TroopStub.TroopObject.Y;
            }
        }

        public override void ReceiveReward(int reward, Resource resource)
        {
            throw new Exception("Why is a defense combat unit receiving rewards dammit?");
        }

        public override void CalculateDamage(decimal dmg, out decimal actualDmg)
        {            
            // if hp is less than 20% of the original total HP(entire group), lastStand kicks in.
            if (Hp < (Hp + DmgRecv) / 5)
            {
                var percent = TroopStub.City.Technologies.GetEffects(EffectCode.LastStand)
                    .Where(tech => BattleFormulas.Current.UnitStatModCheck(Stats.Base, TroopBattleGroup.Attack, (string)tech.Value[1]))
                    .DefaultIfEmpty()
                    .Max(x => x == null ? 0 : (int)x.Value[0]);

                dmg = dmg * (100 - percent) / 100;
            }

            actualDmg = Math.Min(Hp, dmg);
        }

        public override void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
        {
            attackPoints = 0;

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
                    dead = count;

                count -= dead;

                attackPoints = Formula.Current.GetUnitKilledAttackPoint(type, lvl, dead);

                // Remove dead units from troop stub
                TroopStub.BeginUpdate();
                TroopStub[formation].Remove(type, dead);
                TroopStub.EndUpdate();
            }

            returning = null;
        }

        public override void ExitBattle()
        {
        }

        public override int CompareTo(object other)
        {
            if (other is ITroopStub)
                return other == TroopStub ? 0 : 1;

            return -1;
        }
    }
}