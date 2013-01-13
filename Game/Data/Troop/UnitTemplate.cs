#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Game.Data.Stats;
using Game.Setup;
using Ninject;
using Persistance;

#endregion

namespace Game.Data.Troop
{
    public class UnitTemplate : IEnumerable<KeyValuePair<ushort, BaseUnitStats>>, IPersistableList
    {
        #region Event

        #region Delegates

        public delegate void UpdateCallback(UnitTemplate template);

        #endregion

        public event UpdateCallback UnitUpdated;

        #endregion

        public const string DB_TABLE = "unit_templates";

        private readonly ICity city;

        private readonly Dictionary<ushort, BaseUnitStats> dict = new Dictionary<ushort, BaseUnitStats>();

        private bool isUpdating;

        public UnitTemplate(ICity city)
        {
            this.city = city;
        }

        public ICity City
        {
            get
            {
                return city;
            }
        }

        public BaseUnitStats this[ushort type]
        {
            get
            {
                BaseUnitStats ret;
                if (dict.TryGetValue(type, out ret))
                {
                    return ret;
                }
                return Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type, 1);
            }
            set
            {
                dict[type] = value;

                if (!isUpdating)
                {
                    UpdateClient();
                }
            }
        }

        public int Size
        {
            get
            {
                return (byte)dict.Count;
            }
        }

        #region IEnumerable<KeyValuePair<ushort,BaseUnitStats>> Members

        public IEnumerator<KeyValuePair<ushort, BaseUnitStats>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        #endregion

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
                return new[] {new DbColumn("city_id", City.Id, DbType.UInt32)};
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
                return new[] {new DbColumn("type", DbType.UInt16), new DbColumn("level", DbType.Byte)};
            }
        }

        public bool DbPersisted { get; set; }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            Dictionary<ushort, BaseUnitStats>.Enumerator itr = dict.GetEnumerator();
            while (itr.MoveNext())
            {
                yield return
                        new[]
                        {
                                new DbColumn("type", itr.Current.Key, DbType.UInt16),
                                new DbColumn("level", itr.Current.Value.Lvl, DbType.Byte),
                        };
            }
        }

        #endregion

        public void BeginUpdate()
        {
            if (isUpdating)
            {
                throw new Exception("Nesting beginupdate");
            }
            isUpdating = true;
        }

        public void EndUpdate()
        {
            isUpdating = false;

            UpdateClient();
        }

        public void DbLoaderAdd(ushort type, BaseUnitStats stats)
        {
            dict[type] = stats;
        }

        private void UpdateClient()
        {
            if (UnitUpdated != null)
            {
                UnitUpdated(this);
            }
        }
    }
}