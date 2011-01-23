#region

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game.Data
{
    public class StructureProperties : IPersistableList
    {
        public const string DB_TABLE = "structure_properties";
        private readonly ListDictionary properties = new ListDictionary();

        private Structure structure;

        public StructureProperties(Structure owner)
        {
            structure = owner;
        }

        public Structure Owner
        {
            get
            {
                return structure;
            }
            set
            {
                structure = value;
            }
        }

        #region IPersistableList Members

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
                return new[] {new DbColumn("structure_id", structure.ObjectId, DbType.UInt32), new DbColumn("city_id", structure.City.Id, DbType.UInt32)};
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
                return new[] {new DbColumn("name", DbType.String), new DbColumn("value", DbType.String), new DbColumn("datatype", DbType.Byte)};
            }
        }

        public bool DbPersisted { get; set; }

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator()
        {
            IDictionaryEnumerator itr = properties.GetEnumerator();

            while (itr.MoveNext())
            {
                byte datatype = DataTypeSerializer.Serialize(itr.Value);

                yield return
                        new[]
                        {
                                new DbColumn("name", itr.Key, DbType.String), new DbColumn("value", itr.Value, DbType.String),
                                new DbColumn("datatype", datatype, DbType.Byte)
                        };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        #endregion

        public bool Contains(object key)
        {
            return properties.Contains(key);
        }

        public void Add(object key, object value)
        {
            structure.CheckUpdateMode();
            properties.Remove(key);
            properties.Add(key, value);
        }

        public void Remove(object key)
        {
            structure.CheckUpdateMode();
            properties.Remove(key);
        }

        public object Get(object key)
        {
            return properties[key];
        }

        public void Clear()
        {
            structure.CheckUpdateMode();
            properties.Clear();
        }
    }
}