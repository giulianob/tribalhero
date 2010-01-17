#region

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game.Data {
    public class StructureProperties : IPersistableList {
        private ListDictionary properties = new ListDictionary();

        private Structure structure;

        public StructureProperties(Structure owner) {
            structure = owner;
        }

        public Structure Owner {
            get { return structure; }
            set { structure = value; }
        }

        public bool contains(object key) {
            return properties.Contains(key);
        }

        public void add(object key, object value) {
            structure.CheckUpdateMode();
            properties.Add(key, value);
        }

        public object get(object key) {
            return properties[key];
        }

        public void clear() {
            structure.CheckUpdateMode();
            properties.Clear();
        }

        #region IPersistable Members

        public const string DB_TABLE = "structure_properties";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                                          new DbColumn("structure_id", structure.ObjectId, DbType.UInt32),
                                          new DbColumn("city_id", structure.City.Id, DbType.UInt32)
                                      };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public DbColumn[] DbColumns {
            get { return new DbColumn[] {}; }
        }

        public DbColumn[] DbListColumns {
            get {
                return new DbColumn[] {
                                          new DbColumn("name", DbType.String), new DbColumn("value", DbType.String),
                                          new DbColumn("datatype", DbType.Byte)
                                      };
            }
        }

        private bool dbPersisted = false;

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
            IDictionaryEnumerator itr = properties.GetEnumerator();

            while (itr.MoveNext()) {
                byte datatype = DataTypeSerializer.Serialize(itr.Value);

                yield return
                    new DbColumn[] {
                                       new DbColumn("name", itr.Key, DbType.String), new DbColumn("value", itr.Value, DbType.String),
                                       new DbColumn("datatype", datatype, DbType.Byte)
                                   };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return properties.GetEnumerator();
        }

        #endregion
    }
}