#region

using System.Collections.Generic;
using System.Data;
using Game.Data.Troop;
using Game.Database;

#endregion

namespace Game.Battle {
    public class ReportedTroops : Dictionary<TroopStub, uint>, IPersistableList {
        private BattleManager battle;

        public ReportedTroops(BattleManager battle) {
            this.battle = battle;
        }

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "reported_troops";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new[] {new DbColumn("city_id", battle.City.Id, DbType.UInt32)}; }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public DbColumn[] DbColumns {
            get { return new DbColumn[] {}; }
        }

        public DbColumn[] DbListColumns {
            get {
                return new[] {
                                          new DbColumn("combat_troop_id", DbType.UInt32),
                                          new DbColumn("troop_stub_city_id", DbType.UInt32), new DbColumn("troop_stub_id", DbType.Byte)
                                      };
            }
        }

        #endregion

        #region IEnumerable<DbColumn[]> Members

        public new IEnumerator<DbColumn[]> GetEnumerator() {
            Enumerator itr = base.GetEnumerator();
            while (itr.MoveNext()) {
                yield return
                    new[] {
                                       new DbColumn("combat_troop_id", itr.Current.Value, DbType.UInt32),
                                       new DbColumn("troop_stub_city_id", itr.Current.Key.City.Id, DbType.UInt32),
                                       new DbColumn("troop_stub_id", itr.Current.Key.TroopId, DbType.Byte)
                                   };
            }
        }

        #endregion
    }

    public class ReportedObjects : List<CombatObject>, IPersistableList {
        private BattleManager battle;

        public ReportedObjects(BattleManager battle) {
            this.battle = battle;
        }

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "reported_objects";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new[] {new DbColumn("city_id", battle.City.Id, DbType.UInt32)}; }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public DbColumn[] DbColumns {
            get { return new DbColumn[] {}; }
        }

        public DbColumn[] DbListColumns {
            get { return new[] {new DbColumn("combat_object_id", DbType.UInt32)}; }
        }

        #endregion

        #region IEnumerable<DbColumn[]> Members

        public new IEnumerator<DbColumn[]> GetEnumerator() {
            Enumerator itr = base.GetEnumerator();
            while (itr.MoveNext())
                yield return new[] {new DbColumn("combat_object_id", itr.Current.Id, DbType.UInt32)};
        }

        #endregion
    }
}