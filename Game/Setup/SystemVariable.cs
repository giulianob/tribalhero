#region

using System.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game.Setup {
    public class SystemVariable : IPersistableObject {
        public SystemVariable(string key, object value) {
            this.key = key;
            this.value = value;
        }

        private string key = string.Empty;

        public string Key {
            get { return key; }
            set { key = value; }
        }

        private object value;

        public object Value {
            get { return value; }
            set { this.value = value; }
        }

        #region IPersistableObject Members

        private bool dbPersisted = false;

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "system_variables";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new DbColumn[] {new DbColumn("name", key, DbType.String)}; }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                                          new DbColumn("value", value, DbType.String),
                                          new DbColumn("datatype", DataTypeSerializer.Serialize(value), DbType.Byte)
                                      };
            }
        }

        #endregion
    }
}