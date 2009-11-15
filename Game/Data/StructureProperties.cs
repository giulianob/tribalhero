using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using Game.Database;
using System.Collections;
using Game.Util;

namespace Game.Data {
    public class StructureProperties : ListDictionary, IPersistableList {
        Structure structure;

        public StructureProperties(Structure owner) {
            this.structure = owner;
        }

        public Structure Owner {
            get { return structure; }
            set { structure = value; }
        }

        #region IPersistable Members
        public const string DB_TABLE = "structure_properties";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("structure_id", structure.ObjectID, System.Data.DbType.UInt32),
                    new DbColumn("city_id", structure.City.CityId, System.Data.DbType.UInt32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { };
            }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] { };
            }
        }

        public DbColumn[] DbListColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("name", System.Data.DbType.String),
                    new DbColumn("value", System.Data.DbType.String),
                    new DbColumn("datatype", System.Data.DbType.Byte)
                };
            }
        }

        bool dbPersisted = false;
        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
            IDictionaryEnumerator itr = GetEnumerator();
            
            while (itr.MoveNext()) {
                byte datatype = DataTypeSerializer.Serialize(itr.Value);

                yield return new DbColumn[] {
                        new DbColumn("name", itr.Key, System.Data.DbType.String),
                        new DbColumn("value", itr.Value, System.Data.DbType.String),
                        new DbColumn("datatype", datatype, System.Data.DbType.Byte)
                    };
            }            
        }

        #endregion
    }
}
