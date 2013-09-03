using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public class StrongholdCombatGroup : CombatGroup
    {
        private readonly BattleOwner owner;

        private readonly IStronghold stronghold;

        public StrongholdCombatGroup(uint battleId, uint id, IStronghold stronghold, IDbManager dbManager)
                : base(battleId, id, dbManager)
        {
            this.stronghold = stronghold;
            owner = new BattleOwner(BattleOwnerType.Stronghold, stronghold.Id);
        }

        #region Persistance

        public const string DB_TABLE = "stronghold_combat_groups";

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
                return new[] {new DbColumn("stronghold_id", stronghold.Id, DbType.UInt32)};
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
                return stronghold.Hash;
            }
        }

        public override object Lock
        {
            get
            {
                return stronghold.Lock;
            }
        }

        public override bool BelongsTo(IPlayer player)
        {
            return false;
        }
    }
}