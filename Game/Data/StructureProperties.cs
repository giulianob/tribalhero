#region

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using Game.Util;
using Persistance;

#endregion

namespace Game.Data
{
    public class StructureProperties : IPersistableList
    {
        private readonly uint cityId;

        private readonly uint structureId;

        public const string DB_TABLE = "structure_properties";

        private readonly ListDictionary properties = new ListDictionary();

        public StructureProperties(uint cityId, uint structureId)
        {
            this.cityId = cityId;
            this.structureId = structureId;
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
                return new[] {new DbColumn("structure_id", structureId, DbType.UInt32), new DbColumn("city_id", cityId, DbType.UInt32)};
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
                        new DbColumn("name", DbType.String), new DbColumn("value", DbType.String),
                        new DbColumn("datatype", DbType.Byte)
                };
            }
        }

        public bool DbPersisted { get; set; }

        public IEnumerable<DbColumn[]> DbListValues()
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

        #endregion

        public bool Contains(object key)
        {
            return properties.Contains(key);
        }

        public void Add(object key, object value)
        {            
            properties.Remove(key);
            properties.Add(key, value);
        }

        public void Remove(object key)
        {
            properties.Remove(key);
        }

        public bool TryGet(object key, out object value)
        {
            if (!properties.Contains(key))
            {
                value = null;
                return false;
            }
            value = properties[key];
            return true;
        }

        public object Get(object key)
        {
            return properties[key];
        }

        public void Clear()
        {
            properties.Clear();
        }
    }
}