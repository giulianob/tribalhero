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
        public const string DB_TABLE = "structure_properties";

        private readonly ListDictionary properties = new ListDictionary();

        private IStructure structure;

        public StructureProperties(IStructure owner)
        {
            structure = owner;
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
                return new[]
                {
                        new DbColumn("city_id", structure.City.Id, DbType.UInt32),
                        new DbColumn("structure_id", structure.ObjectId, DbType.UInt32)
                };
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
                        new DbColumn("name", DbType.String), 
                        new DbColumn("value", DbType.String),
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
                                new DbColumn("name", itr.Key, DbType.String), 
                                new DbColumn("value", itr.Value, DbType.String),
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
            structure.CheckUpdateMode();
            properties.Remove(key);
            properties.Add(key, value);
        }

        public void Remove(object key)
        {
            structure.CheckUpdateMode();
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
            structure.CheckUpdateMode();
            properties.Clear();
        }
    }
}