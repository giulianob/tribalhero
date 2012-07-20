using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Troop;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public class CityDefensiveCombatGroup : CombatGroup
    {
        public ITroopStub TroopStub { get; set; }

        private readonly BattleOwner owner;

        private readonly uint id;

        public override uint Id
        {
            get
            {
                return id;
            }
        }

        public override byte TroopId
        {
            get
            {
                return TroopStub.TroopId;
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

        public CityDefensiveCombatGroup(uint id, ITroopStub troopStub, IDbManager dbManager) : base(dbManager)
        {
            owner = new BattleOwner(BattleOwnerType.City, troopStub.City.Id);
            TroopStub = troopStub;
            this.id = id;
        }

        #region Persistance

        public const string DB_TABLE = "city_defensive_combat_groups";

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
                return new[] {new DbColumn("id", Id, DbType.UInt32)};
            }
        }

        public override DbColumn[] DbColumns
        {
            get
            {
                return new[] {new DbColumn("troop_stub_id", TroopStub.TroopId, DbType.UInt32), new DbColumn("city_id", TroopStub.City.Id, DbType.UInt32)};
            }
        }

        #endregion
        
        public override int Hash
        {
            get
            {
                return TroopStub.City.Hash;
            }
        }

        public override object Lock
        {
            get
            {
                return TroopStub.City.Lock;
            }
        }

        public override bool BelongsTo(IPlayer player)
        {        
            return TroopStub.City.Owner == player;        
        }

        public ICity City
        {
            get
            {
                return TroopStub.City;
            }
        }
    }
}
