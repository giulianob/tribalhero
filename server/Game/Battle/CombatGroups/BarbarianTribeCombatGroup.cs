using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.BarbarianTribe;

using Game.Data.Tribe;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public class BarbarianTribeCombatGroup : CombatGroup
    {
        private readonly BattleOwner owner;

        private readonly IBarbarianTribe barbarianTribe;

        public BarbarianTribeCombatGroup(uint battleId, uint id, IBarbarianTribe barbarianTribe, IDbManager dbManager)
                : base(battleId, id, dbManager)
        {
            this.barbarianTribe = barbarianTribe;
            owner = new BattleOwner(BattleOwnerType.BarbarianTribe, barbarianTribe.ObjectId);
        }

        #region Persistance

        public const string DB_TABLE = "barbarian_tribe_combat_groups";

        public override IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public override bool DbPersisted { get; set; }

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

        public override DbColumn[] DbColumns
        {
            get
            {
                return new[] {new DbColumn("barbarian_tribe_id", barbarianTribe.ObjectId, DbType.UInt32)};
            }
        }

        #endregion

        public override ushort TroopId
        {
            get
            {
                return 1;
            }
        }

        public override Resource GroupLoot
        {
            get
            {
                return new Resource();
            }
        }

        public override BattleOwner Owner
        {
            get
            {
                return owner;
            }
        }

        public override ITribe Tribe
        {
            get
            {
                return null;
            }
        }

        public override int Hash
        {
            get
            {
                return barbarianTribe.Hash;
            }
        }

        public override object Lock
        {
            get
            {
                return barbarianTribe.Lock;
            }
        }

        public override bool BelongsTo(IPlayer player)
        {
            return false;
        }
    }
}