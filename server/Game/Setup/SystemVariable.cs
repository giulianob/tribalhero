#region

using System.Collections.Generic;
using System.Data;
using Game.Util;
using Persistance;

#endregion

namespace Game.Setup
{
    public class SystemVariable : IPersistableObject
    {
        public const string DB_TABLE = "system_variables";

        private string key = string.Empty;

        private object value;

        protected SystemVariable()
        {
        }

        public SystemVariable(string key, object value)
        {
            this.key = key;
            this.value = value;
        }

        public virtual string Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        public virtual object Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        #region IPersistableObject Members

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
                return new[] {new DbColumn("name", key, DbType.String)};
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
                return new[]
                {
                        new DbColumn("value", value, DbType.String),
                        new DbColumn("datatype", DataTypeSerializer.Serialize(value), DbType.Byte)
                };
            }
        }

        #endregion
    }
}