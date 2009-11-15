using System;
using System.Collections.Generic;
using System.Text;
using Game.Fighting;
using Game.Database;
using Game.Util;
using Game.Data.Troop;

namespace Game.Data {
    public class TroopStub : IEnumerable<Formation>, IEnumerable<KeyValuePair<FormationType, Formation>>, IPersistableList, ILockable {

        public enum TroopState {
            IDLE = 0,
            BATTLE = 1,
            STATIONED = 2,
            BATTLE_STATIONED = 3,
            MOVING = 4
        }

        protected Dictionary<FormationType, Formation> data = new Dictionary<FormationType, Formation>();
        bool isUpdating = false;
        bool isDirty = false;

        object objLock = new object();

        #region Properties
        TroopTemplate troopTemplate;
        public TroopTemplate TroopTemplate {
            get { return troopTemplate; }
            set { troopTemplate = value; }
        }

        TroopManager troopManager;
        public TroopManager TroopManager {
            get { return troopManager; }
            set { troopManager = value; }
        }

        public Formation this[FormationType type] {
            get { return data[type]; }
            set { data[type] = value; }
        }

        TroopState state = TroopState.IDLE;
        public TroopState State {
            get { return state; }
            set { state = value; isDirty = true; }
        }

        City city;
        public City City {
            get { return city; }
            set { city = value; }
        }

        byte stationedTroopId;
        public byte StationedTroopId {
            get { return stationedTroopId; }
            set { stationedTroopId = value; }
        }

        City stationedCity;
        public City StationedCity {
            get { return stationedCity; }
            set { stationedCity = value; }
        }

        TroopObject troopObject;
        public TroopObject TroopObject {
            get { return troopObject; }
            set { troopObject = value; }
        }

        byte troopId;
        public byte TroopId {   
            get { return troopId; }
            set { troopId = value; }
        }

        public byte FormationCount {
            get { return (byte)data.Count; }
        }

        public ushort TotalCount {
            get {
                ushort count = 0;

                lock (objLock) {
                    foreach (Formation formation in this.data.Values) {
                        count += (ushort)formation.Count;
                    }
                }

                return count;
            }
        }
        public int TotalHP {
            get {
                int count = 0;

                lock (objLock) {
                    foreach (Formation formation in this.data.Values) {
                        foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                            count += (kvp.Value * city.Template[kvp.Key].stats.MaxHp);
                        }
                    }
                }

                return count;
            }
        }

        public int Upkeep {
            get {
                int count = 0;
                lock (objLock) {
                    foreach (Formation formation in this.data.Values) {
                        foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                            count += (kvp.Value * city.Template[kvp.Key].upkeep);
                        }
                    }
                }

                return count;
            }
        }

        public void Starve() {
            lock (objLock) {
                foreach (Formation formation in this.data.Values) {
                    foreach (KeyValuePair<ushort,ushort> kvp in new Dictionary<ushort,ushort>(formation)) {
                        if (kvp.Value == 1) {
                            formation.Remove(kvp.Key);
                        } else {
                            formation[kvp.Key] = (ushort)(kvp.Value * 95 / 100);
                        }
                    }
                }

            }
        }

        public Dictionary<ushort, int> TotalUnitByType() {
            lock (objLock) {
                Dictionary<ushort, int> dict = new Dictionary<ushort, int>();
                foreach (Formation formation in this.data.Values) {
                    foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                        if (dict.ContainsKey(kvp.Key)) {
                            dict[kvp.Key] += kvp.Value;
                        }
                        else {
                            dict[kvp.Key] = kvp.Value;
                        }
                    }
                }

                return dict;
            }
        }

        public bool Equal(TroopStub troopStub) {
            lock (objLock) {
                Dictionary<ushort, int> dict = TotalUnitByType();
                foreach (Formation formation in troopStub.data.Values) {
                    foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                        if (dict.ContainsKey(kvp.Key)) {
                            if ((dict[kvp.Key] -= kvp.Value) < 0) {
                                return false;
                            }
                            else if (dict[kvp.Key] == 0) {
                                dict.Remove(kvp.Key);
                            }
                        }
                        else {
                            return false;
                        }
                    }
                }

                return dict.Count == 0;
            }
        }

        #endregion

        #region Events
        public delegate void OnUnitUpdate(TroopStub stub);
        public event OnUnitUpdate UnitUpdate;
        #endregion

        public TroopStub(TroopManager troopManager) {
            this.troopManager = troopManager;            
        }

        public void FireUpdated() {
            if (!Global.FireEvents) return;

            if (!isUpdating) {
                if (UnitUpdate != null)
                    UnitUpdate(this);
                isDirty = false;
            }
            else {
                isDirty = true;
            }
        }

        public void BeginUpdate() {
            isUpdating = true;
        }

        public void EndUpdate() {
            isUpdating = false;

            if (isDirty)
                FireUpdated();
        }

        public bool addFormation(FormationType type) {
            lock (objLock) {
                if (data.ContainsKey(type)) return false;
                data.Add(type, new Formation(this));
            }

            return true;
        }

        public bool add(TroopStub stub) {
            bool alreadyUpdating = isUpdating;

            if (!alreadyUpdating)
                BeginUpdate();

            lock (objLock) {
                foreach (KeyValuePair<FormationType, Formation> kvp in (IEnumerable<KeyValuePair<FormationType, Formation>>)stub) {
                    Formation formation;
                    if (!data.TryGetValue(kvp.Key, out formation)) {
                        formation = new Formation(this);
                        data.Add(kvp.Key, formation);
                    }
                    formation.Add(kvp.Value);
                }
            }

            if (!alreadyUpdating)
                EndUpdate();

            return true;
        }

        public bool addUnit(FormationType formation_type, ushort type, ushort count) {
            lock (objLock) {
                Formation formation;
                if (data.TryGetValue(formation_type, out formation)) {
                    formation.add(type, count);
                    return true;
                }
            }
            return false;
        }

        public bool remove(TroopStub troop) {
            lock (objLock) {
                if (!hasEnough(troop)) return false;

                bool alreadyUpdating = isUpdating;

                if (!alreadyUpdating)
                    BeginUpdate();

                foreach (KeyValuePair<FormationType, Formation> kvp in (IEnumerable<KeyValuePair<FormationType, Formation>>)troop) {
                    Formation formation;
                    if (!data.TryGetValue(kvp.Key, out formation)) return false;
                    foreach (KeyValuePair<ushort, ushort> unit in kvp.Value) {
                        formation.remove(unit.Key, unit.Value);
                    }
                }

                if (!alreadyUpdating)
                    EndUpdate();
            }

            return true;
        }

        public ushort removeUnit(FormationType formation_type, ushort type, ushort count) {
            lock (objLock) {
                Formation formation;
                if (data.TryGetValue(formation_type, out formation)) {
                    ushort removed = formation.remove(type, count);
                    if (removed > 0) {
                        FireUpdated();
                        return removed;
                    }

                }
                return 0;
            }
        }


        public void removeAllUnits() {
            lock (objLock) {
                foreach (KeyValuePair<FormationType, Formation> kvp in data) {
                    kvp.Value.Clear();
                }
            }
        }
        
        public bool hasEnough(TroopStub troop) {
            lock (objLock) {
                foreach (KeyValuePair<FormationType, Formation> kvp in (IEnumerable<KeyValuePair<FormationType, Formation>>)troop) {
                    Formation formation;
                    if (!data.TryGetValue(kvp.Key, out formation)) return false;
                    foreach (KeyValuePair<ushort, ushort> unit in kvp.Value) {
                        ushort count;
                        if (!formation.TryGetValue(unit.Key, out count)) return false;
                        if (count < unit.Value) return false;
                    }
                }
            }
            return true;
        }

        public void print() {
            Dictionary<FormationType, Formation>.Enumerator itr = data.GetEnumerator();
            while (itr.MoveNext()) {
                Global.Logger.Info(string.Format("Formation type: " + Enum.GetName(typeof(FormationType), itr.Current.Key)));
                Dictionary<ushort, ushort>.Enumerator itr2 = itr.Current.Value.GetEnumerator();
                while (itr2.MoveNext()) {
                    Global.Logger.Error(string.Format("\t\tType[{0}] : Count[{1}]", itr2.Current.Key, itr2.Current.Value));
                }
            }
        }

        public bool TryGetValue(FormationType formationType, out Formation formation) {
            return data.TryGetValue(formationType, out formation);
        }

        #region IEnumerable<PaperFormation> Members

        public IEnumerator<Formation> GetEnumerator() {
            return data.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return data.GetEnumerator();
        }

        #endregion

        #region IEnumerable<KeyValuePair<FormationType,PaperFormation>> Members

        IEnumerator<KeyValuePair<FormationType, Formation>> IEnumerable<KeyValuePair<FormationType, Formation>>.GetEnumerator() {
            return data.GetEnumerator();
        }

        #endregion

        #region IPersistable Members
        public ushort getFormationBits() {
            ushort mask = 0;
            foreach (FormationType type in data.Keys) {
                mask += (ushort)Math.Pow(2, (double)type);
            }
            return mask;
        }

        public const string DB_TABLE = "troop_stubs";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("id", troopId, System.Data.DbType.UInt32),
                    new DbColumn("city_id", troopManager.City.CityId, System.Data.DbType.UInt32)                    
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
                return new DbColumn[] {
                    new DbColumn("stationed_city_id", stationedCity != null ? stationedCity.CityId : 0, System.Data.DbType.UInt32),
                    new DbColumn("state", (byte)state, System.Data.DbType.Byte),
                    new DbColumn("formations", getFormationBits(), System.Data.DbType.UInt16)
                };
            }
        }

        public DbColumn[] DbListColumns {
            get {
                return new DbColumn[] {                    
                    new DbColumn("formation_type", System.Data.DbType.Byte),
                    new DbColumn("type", System.Data.DbType.UInt16),
                    new DbColumn("count", System.Data.DbType.UInt16)
                };
            }
        }

        bool dbPersisted = false;
        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
            lock (objLock) {
                Dictionary<FormationType, Formation>.Enumerator itr = data.GetEnumerator();
                while (itr.MoveNext()) {
                    Dictionary<ushort, ushort>.Enumerator itr2 = itr.Current.Value.GetEnumerator();
                    while (itr2.MoveNext()) {
                        yield return new DbColumn[] {
                            new DbColumn("formation_type", (int)itr.Current.Key, System.Data.DbType.Byte),
                            new DbColumn("type", itr2.Current.Key, System.Data.DbType.UInt16),
                            new DbColumn("count", itr2.Current.Value, System.Data.DbType.UInt16)
                        };
                    }
                }
            }
        }

        #endregion

        #region ILockable Members

        public int Hash {
            get { return unchecked((int)City.Owner.PlayerId); }
        }

        public object Lock {
            get { return City.Lock; }
        }

        #endregion

    }
}
