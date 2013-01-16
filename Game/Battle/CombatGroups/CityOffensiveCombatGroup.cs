using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public class CityOffensiveCombatGroup : CombatGroup
    {
        private readonly BattleOwner owner;

        public CityOffensiveCombatGroup(uint battleId, uint id, ITroopObject troopObject, IDbManager dbManager)
                : base(battleId, id, dbManager)
        {
            owner = new BattleOwner(BattleOwnerType.City, troopObject.City.Id);
            TroopObject = troopObject;
        }

        #region Persistance

        public const string DB_TABLE = "city_offensive_combat_groups";

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
                return new[]
                {
                        new DbColumn("troop_object_id", TroopObject.ObjectId, DbType.UInt32),
                        new DbColumn("city_id", TroopObject.City.Id, DbType.UInt32)
                };
            }
        }

        public override int Hash
        {
            get
            {
                return TroopObject.City.Hash;
            }
        }

        public override object Lock
        {
            get
            {
                return TroopObject.City.Lock;
            }
        }

        #endregion

        public ITroopObject TroopObject { get; private set; }

        public override byte TroopId
        {
            get
            {
                return TroopObject.Stub.TroopId;
            }
        }

        public override Resource GroupLoot
        {
            get
            {
                return TroopObject.Stats.Loot;
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
                return TroopObject.City.Owner.IsInTribe ? TroopObject.City.Owner.Tribesman.Tribe : null;
            }
        }

        public ICity City
        {
            get
            {
                return TroopObject.City;
            }
        }

        public override bool BelongsTo(IPlayer player)
        {
            return TroopObject.City.Owner == player;
        }
    }
}