using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using Game.Database;
using Game.Setup;
using Game.Comm;

namespace Game.Fighting {
    public class UnitTemplate : IEnumerable<KeyValuePair<ushort, UnitStats>>, IPersistableList {
        #region Event
        public delegate void UpdateCallback(UnitTemplate template);
        public event UpdateCallback UnitUpdated;
        #endregion

        Dictionary<ushort, UnitStats> dict = new Dictionary<ushort, UnitStats>();

        bool isUpdating = false;

        City city;
        public City City {
            get { return city; }
        }

        public UnitStats this[ushort type] {
            get {
                UnitStats ret;
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

        public void dbLoaderAdd(ushort type, UnitStats stats) {
            dict[type] = stats;
        }

        void updateClient() {
            if (UnitUpdated != null)
                UnitUpdated(this);
        }

        #region IEnumerable<KeyValuePair<ushort,UnitStats>> Members

        public IEnumerator<KeyValuePair<ushort, UnitStats>> GetEnumerator() {
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
        #endregion

        #region IEnumerable<DbColumn[]> Members

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
            Dictionary<ushort, UnitStats>.Enumerator itr = dict.GetEnumerator();
            while (itr.MoveNext()) {
                yield return new DbColumn[] {
                    new DbColumn("type", itr.Current.Key, System.Data.DbType.UInt16),
                    new DbColumn("level", itr.Current.Value.lvl, System.Data.DbType.Byte),
                };
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<ushort,UnitStats>> Members

        IEnumerator<KeyValuePair<ushort, UnitStats>> IEnumerable<KeyValuePair<ushort, UnitStats>>.GetEnumerator() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
