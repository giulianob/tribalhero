#region

using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Troop;
using Persistance;

#endregion

namespace Game.Battle
{
    public class ReportedTroops : Dictionary<TroopStub, uint>, IPersistableList
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

        public DbDependency[] DbDependencies
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

        public DbColumn[] DbListColumns
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

        public new IEnumerator<DbColumn[]> GetEnumerator()
        {
            Enumerator itr = GetBaseEnumerator();
            while (itr.MoveNext())
            {
                yield return
                        new[]
                        {
                                new DbColumn("combat_troop_id", itr.Current.Value, DbType.UInt32),
                                new DbColumn("troop_stub_city_id", itr.Current.Key.City.Id, DbType.UInt32),
                                new DbColumn("troop_stub_id", itr.Current.Key.TroopId, DbType.Byte)
                        };
            }
        }

        #endregion

        private Enumerator GetBaseEnumerator()
        {
            return base.GetEnumerator();
        }
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

        public DbDependency[] DbDependencies
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

        public DbColumn[] DbListColumns
        {
            get
            {
                return new[] {new DbColumn("combat_object_id", DbType.UInt32)};
            }
        }

        public new IEnumerator<DbColumn[]> GetEnumerator()
        {
            Enumerator itr = GetBaseEnumerator();
            while (itr.MoveNext())
                yield return new[] {new DbColumn("combat_object_id", itr.Current.Id, DbType.UInt32)};
        }

        #endregion

        private Enumerator GetBaseEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}