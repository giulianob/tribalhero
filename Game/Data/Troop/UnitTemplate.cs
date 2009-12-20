using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using Game.Database;
using Game.Setup;
using Game.Comm;
using Game.Data.Stats;

namespace Game.Fighting {
    public class UnitTemplate : IEnumerable<KeyValuePair<ushort, BaseUnitStats>>, IPersistableList {
        #region Event
        public delegate void UpdateCallback(UnitTemplate template);
        public event UpdateCallback UnitUpdated;
        #endregion

        Dictionary<ushort, BaseUnitStats> dict = new Dictionary<ushort, BaseUnitStats>();

        bool isUpdating = false;

        City city;
        public City City {
            get { return city; }
        }

        public BaseUnitStats this[ushort type] {
            get {
                BaseUnitStats ret;
                if (dict.TryGetValue(type, out ret)) return ret;
                return UnitFactory.getUnitStats(type,1);
            }
            set {
                dict[type] = value;

                if (!isUpdating)
                    updateClient();
            }
        }

        public int Size {
            get { return (byte)dict.Count; }
        }

        public UnitTemplate(City city) {
            this.city = city;
        }

        public void BeginUpdate() {
            isUpdating = true;
        }

        public void EndUpdate() {
            isUpdating = false;

            updateClient();            
        }

        public void dbLoaderAdd(ushort type, BaseUnitStats stats) {
            dict[type] = stats;
        }

        void updateClient() {
            if (UnitUpdated != null)
                UnitUpdated(this);
        }

        #region IEnumerable<KeyValuePair<ushort,UnitStats>> Members

        public IEnumerator<KeyValuePair<ushort, BaseUnitStats>> GetEnumerator() {
            return dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return dict.GetEnumerator();
        }

        #endregion

        #region IPersistable Members
        public const string DB_TABLE = "unit_templates";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("city_id", City.CityId, System.Data.DbType.UInt32)
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
                    new DbColumn("type", System.Data.DbType.UInt16),
                    new DbColumn("level", System.Data.DbType.Byte)
                };
            }
        }

        bool dbPersisted = false;
        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
            Dictionary<ushort, BaseUnitStats>.Enumerator itr = dict.GetEnumerator();
            while (itr.MoveNext()) {
                yield return new DbColumn[] {
                    new DbColumn("type", itr.Current.Key, System.Data.DbType.UInt16),
                    new DbColumn("level", itr.Current.Value.Lvl, System.Data.DbType.Byte),
                };
            }
        }

        #endregion
    }
}
