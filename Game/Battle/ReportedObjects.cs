#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Persistance;

#endregion

namespace Game.Battle
{
    public class ReportedTroops : Dictionary<ITroopStub, uint>, IPersistableList
    {
        public const string DB_TABLE = "reported_troops";
        private readonly ICity city;

        public ReportedTroops(ICity city)
        {
            this.city = city;
        }

        #region IPersistableList Members

        public bool DbPersisted { get; set; }

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("city_id", city.Id, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new DbColumn[] {};
            }
        }

        public IEnumerable<DbColumn> DbListColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("combat_troop_id", DbType.UInt32), new DbColumn("troop_stub_city_id", DbType.UInt32),
                               new DbColumn("troop_stub_id", DbType.Byte)
                       };
            }
        }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            return this.Select(reportedObject => new[]
                                                 {
                                                         new DbColumn("combat_troop_id", reportedObject.Value, DbType.UInt32),
                                                         new DbColumn("troop_stub_city_id", reportedObject.Key.City.Id, DbType.UInt32),
                                                         new DbColumn("troop_stub_id", reportedObject.Key.TroopId, DbType.Byte)
                                                 });
        }

        #endregion
    }

    public class ReportedObjects : List<CombatObject>, IPersistableList
    {
        public const string DB_TABLE = "reported_objects";
        private readonly ICity city;

        public ReportedObjects(ICity city)
        {
            this.city = city;
        }

        #region IPersistableList Members

        public bool DbPersisted { get; set; }

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("city_id", city.Id, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new DbColumn[] {};
            }
        }

        public IEnumerable<DbColumn> DbListColumns
        {
            get
            {
                return new[] {new DbColumn("combat_object_id", DbType.UInt32)};
            }
        }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            return this.Select(reportedObject => new[] {new DbColumn("combat_object_id", reportedObject.Id, DbType.UInt32)});
        }

        #endregion
    }
}