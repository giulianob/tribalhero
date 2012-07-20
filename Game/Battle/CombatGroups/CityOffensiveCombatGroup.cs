using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Troop;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public class CityOffensiveCombatGroup : CombatGroup, IReportView
    {
        public ITroopObject TroopObject { get; private set; }

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

        public CityOffensiveCombatGroup(uint id, ITroopObject troopObject, IDbManager dbManager) : base(dbManager)
        {
            owner = new BattleOwner(BattleOwnerType.City, troopObject.City.Id);
            TroopObject = troopObject;
            this.id = id;
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
                return new[] {new DbColumn("id", Id, DbType.UInt32)};
            }
        }

        public override DbColumn[] DbColumns
        {
            get
            {
                return new[] {new DbColumn("troop_stub_id", TroopObject.Stub.TroopId, DbType.UInt32), new DbColumn("city_id", TroopObject.City.Id, DbType.UInt32)};
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
        
        public override bool BelongsTo(IPlayer player)
        {        
            return TroopObject.City.Owner == player;        
        }

        public ICity City
        {
            get
            {
                return TroopObject.City;
            }
        }
    }
}
