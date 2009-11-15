using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;
using Game.Data;

namespace Game.Battle {
    public class ReportedTroops : Dictionary<TroopStub, uint>, IPersistableList {
        BattleManager battle;

        public ReportedTroops(BattleManager battle) {
            this.battle = battle;
        }

        #region IPersistableObject Members
        bool dbPersisted = false;

        public bool DbPersisted {
            get {
                return dbPersisted;
            }
            set {
                dbPersisted = value;
            }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "reported_troops";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("city_id", battle.City.CityId, System.Data.DbType.UInt32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] { }; }
        }

        public DbColumn[] DbColumns {
            get { return new DbColumn[] { }; }
        }

        public DbColumn[] DbListColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("combat_troop_id", System.Data.DbType.UInt32),
                    new DbColumn("troop_stub_city_id", System.Data.DbType.UInt32),
                    new DbColumn("troop_stub_id", System.Data.DbType.Byte)
                };
            }
        }

        #endregion

        #region IEnumerable<DbColumn[]> Members

        public new IEnumerator<DbColumn[]> GetEnumerator() {
            Enumerator itr = base.GetEnumerator();
            while (itr.MoveNext()) {
                yield return new DbColumn[] {
                    new DbColumn("combat_troop_id", itr.Current.Value, System.Data.DbType.UInt32),
                    new DbColumn("troop_stub_city_id", itr.Current.Key.City.CityId, System.Data.DbType.UInt32),
                    new DbColumn("troop_stub_id", itr.Current.Key.TroopId, System.Data.DbType.Byte)
                };
            }
        }

        #endregion
    }

    public class ReportedObjects : List<CombatObject>, IPersistableList {

        BattleManager battle;

        public ReportedObjects(BattleManager battle) {
            this.battle = battle;
        }

        #region IPersistableObject Members
        bool dbPersisted = false;

        public bool DbPersisted {
            get {
                return dbPersisted;
            }
            set {
                dbPersisted = value;
            }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "reported_objects";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("city_id", battle.City.CityId, System.Data.DbType.UInt32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] { }; }
        }

        public DbColumn[] DbColumns {
            get { return new DbColumn[] { }; }
        }

        public DbColumn[] DbListColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("combat_object_id", System.Data.DbType.UInt32)
                };
            }
        }

        #endregion

        #region IEnumerable<DbColumn[]> Members

        public new IEnumerator<DbColumn[]> GetEnumerator() {
            Enumerator itr = base.GetEnumerator();
            while (itr.MoveNext()) {
                yield return new DbColumn[] {
                    new DbColumn("combat_object_id", itr.Current.Id, System.Data.DbType.UInt32)
                };
            }
        }

        #endregion
    }
}
