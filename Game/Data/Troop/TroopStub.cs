#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Database;
using Game.Util;

#endregion

namespace Game.Data.Troop
{
    public enum TroopState
    {
        Idle = 0,
        Battle = 1,
        Stationed = 2,
        BattleStationed = 3,
        Moving = 4,
        ReturningHome = 5
    }

    public enum TroopBattleGroup
    {
        Local = 0,
        Attack = 1,
        Defense = 2,
    }

    public class TroopStub : IEnumerable<Formation>, IPersistableList, ILockable
    {
        public const string DB_TABLE = "troop_stubs";
        private readonly object objLock = new object();
        protected Dictionary<FormationType, Formation> data = new Dictionary<FormationType, Formation>();
        private bool isDirty;
        private bool isUpdating;

        #region Properties

        private TroopState state = TroopState.Idle;
        private City stationedCity;
        private byte troopId;
        private TroopObject troopObject;
        public TroopTemplate Template { get; private set; }

        public TroopManager TroopManager { get; set; }

        public Formation this[FormationType type]
        {
            get
            {
                return data[type];
            }
            set
            {
                CheckUpdateMode();
                data[type] = value;
            }
        }

        public TroopState State
        {
            get
            {
                return state;
            }
            set
            {
                CheckUpdateMode();
                state = value;
                isDirty = true;
            }
        }

        public City City
        {
            get
            {
                return TroopManager == null ? null : TroopManager.City;
            }
        }

        public byte StationedTroopId { get; set; }

        public City StationedCity
        {
            get
            {
                return stationedCity;
            }
            set
            {
                CheckUpdateMode();
                stationedCity = value;
            }
        }

        public TroopObject TroopObject
        {
            get
            {
                return troopObject;
            }
            set
            {
                CheckUpdateMode(false);
                troopObject = value;
            }
        }

        public byte TroopId
        {
            get
            {
                return troopId;
            }
            set
            {
                CheckUpdateMode();
                troopId = value;
            }
        }

        public byte FormationCount
        {
            get
            {
                return (byte)data.Count;
            }
        }

        public ushort TotalCount
        {
            get
            {
                ushort count = 0;

                lock (objLock)
                {
                    foreach (var formation in data.Values)
                        count += (ushort)formation.Sum(x => x.Value);
                }

                return count;
            }
        }

        public int TotalHp
        {
            get
            {
                int count = 0;

                lock (objLock)
                {
                    foreach (var formation in data.Values)
                    {
                        foreach (var kvp in formation)
                            count += (kvp.Value*City.Template[kvp.Key].Battle.MaxHp);
                    }
                }

                return count;
            }
        }

        /// <summary>
        ///   Returns the sum of the upkeep for all units in troop stub
        /// </summary>
        public int Value
        {
            get
            {
                int count = 0;

                lock (objLock)
                {
                    foreach (var formation in data.Values)
                        count += formation.Sum(kvp => City.Template[kvp.Key].Upkeep*kvp.Value);
                }

                return count;
            }
        }

        public int Upkeep
        {
            get
            {
                int count = 0;
                lock (objLock)
                {
                    foreach (var formation in data.Values)
                    {
                        foreach (var kvp in formation)
                            count += (int)((kvp.Value * City.Template[kvp.Key].Upkeep) * (formation.Type == FormationType.Garrison ? 1.25 : 1));
                    }
                }

                return count;
            }
        }

        public int Carry
        {
            get
            {
                int count = 0;
                lock (objLock)
                {
                    foreach (var formation in data.Values)
                    {
                        foreach (var kvp in formation)
                            count += (kvp.Value*City.Template[kvp.Key].Battle.Carry);
                    }
                }

                return count;
            }
        }

        public bool IsDefault()
        {
            return TroopId == 1;
        }

        public void Starve(int percent = 5)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (var formation in data.Values)
                {
                    foreach (var kvp in new Dictionary<ushort, ushort>(formation))
                    {
                        var newCount = (ushort)(kvp.Value*(100-percent)/100);

                        if (newCount == 0)
                            formation.Remove(kvp.Key);
                        else
                            formation[kvp.Key] = newCount;
                    }
                }

                FireUpdated();
            }
        }

        public Dictionary<ushort, int> TotalUnitByType()
        {
            lock (objLock)
            {
                var dict = new Dictionary<ushort, int>();
                foreach (var formation in data.Values)
                {
                    foreach (var kvp in formation)
                    {
                        if (dict.ContainsKey(kvp.Key))
                            dict[kvp.Key] += kvp.Value;
                        else
                            dict[kvp.Key] = kvp.Value;
                    }
                }

                return dict;
            }
        }

        public bool Equal(TroopStub troopStub, params FormationType[] ignoreFormations)
        {
            lock (objLock)
            {
                Dictionary<ushort, int> dict = TotalUnitByType();
                foreach (var formation in troopStub.data.Values)
                {
                    if (ignoreFormations.Contains(formation.Type))
                        continue;

                    foreach (var kvp in formation)
                    {
                        if (!dict.ContainsKey(kvp.Key))
                            return false;

                        if ((dict[kvp.Key] -= kvp.Value) < 0)
                            return false;

                        if (dict[kvp.Key] == 0)
                            dict.Remove(kvp.Key);
                    }
                }

                return dict.Count == 0;
            }
        }

        public bool Equal(TroopStub troopStub)
        {
            lock (objLock)
            {
                Dictionary<ushort, int> dict = TotalUnitByType();
                foreach (var formation in troopStub.data.Values)
                {
                    foreach (var kvp in formation)
                    {
                        if (!dict.ContainsKey(kvp.Key))
                            return false;

                        if ((dict[kvp.Key] -= kvp.Value) < 0)
                            return false;

                        if (dict[kvp.Key] == 0)
                            dict.Remove(kvp.Key);
                    }
                }

                return dict.Count == 0;
            }
        }

        #endregion

        #region Events

        #region Delegates

        public delegate void OnUnitUpdate(TroopStub stub);

        #endregion

        public event OnUnitUpdate UnitUpdate;

        #endregion

        public TroopStub()
        {
            Template = new TroopTemplate(this);
        }

        #region IEnumerable<Formation> Members

        public IEnumerator<Formation> GetEnumerator()
        {
            return data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        #endregion

        #region ILockable Members

        public int Hash
        {
            get
            {
                return unchecked((int)City.Owner.PlayerId);
            }
        }

        public object Lock
        {
            get
            {
                return City.Lock;
            }
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
                return new[] {new DbColumn("id", troopId, DbType.UInt32), new DbColumn("city_id", TroopManager.City.Id, DbType.UInt32)};
            }
        }

        public DbDependency[] DbDependencies
        {
            get
            {
                return new[] {new DbDependency("Template", true, true)};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("stationed_city_id", stationedCity != null ? stationedCity.Id : 0, DbType.UInt32),
                               new DbColumn("state", (byte)state, DbType.Byte), new DbColumn("formations", GetFormationBits(), DbType.UInt16)
                       };
            }
        }

        public DbColumn[] DbListColumns
        {
            get
            {
                return new[] {new DbColumn("formation_type", DbType.Byte), new DbColumn("type", DbType.UInt16), new DbColumn("count", DbType.UInt16)};
            }
        }

        public bool DbPersisted { get; set; }

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator()
        {
            lock (objLock)
            {
                Dictionary<FormationType, Formation>.Enumerator itr = data.GetEnumerator();
                while (itr.MoveNext())
                {
                    Dictionary<ushort, ushort>.Enumerator itr2 = itr.Current.Value.GetEnumerator();
                    while (itr2.MoveNext())
                    {
                        yield return
                                new[]
                                {
                                        new DbColumn("formation_type", (int)itr.Current.Key, DbType.Byte), new DbColumn("type", itr2.Current.Key, DbType.UInt16),
                                        new DbColumn("count", itr2.Current.Value, DbType.UInt16)
                                };
                    }
                }
            }
        }

        #endregion

        public void FireUpdated()
        {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            isDirty = true;
        }

        private void CheckUpdateMode(bool checkStationedCity = true)
        {
            if (!Global.FireEvents || City == null)
                return;

            if (!isUpdating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(this);

            if (checkStationedCity && stationedCity != null)
                MultiObjectLock.ThrowExceptionIfNotLocked(stationedCity);
        }

        public void BeginUpdate()
        {
            if (isUpdating)
                throw new Exception("Nesting beginupdates");

            isUpdating = true;
            isDirty = false;
        }

        public void EndUpdate()
        {
            isUpdating = false;

            if (isDirty)
                UnitUpdate(this);
        }

        public bool AddFormation(FormationType type)
        {
            lock (objLock)
            {
                CheckUpdateMode();
                if (data.ContainsKey(type))
                    return false;
                data.Add(type, new Formation(type, this));

                FireUpdated();
            }

            return true;
        }

        public bool Add(TroopStub stub)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (var stubFormation in stub)
                {
                    Formation targetFormation;
                    if (!data.TryGetValue(stubFormation.Type, out targetFormation))
                    {
                        targetFormation = new Formation(stubFormation.Type, this);
                        data.Add(stubFormation.Type, targetFormation);
                    }

                    targetFormation.Add(stubFormation);
                }

                FireUpdated();
            }

            return true;
        }

        public bool AddUnit(FormationType formationType, ushort type, ushort count)
        {
            lock (objLock)
            {
                Formation formation;
                if (data.TryGetValue(formationType, out formation))
                {
                    formation.Add(type, count);
                    return true;
                }

                FireUpdated();
            }

            return false;
        }

        public bool Remove(TroopStub troop)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                if (!HasEnough(troop))
                    return false;

                foreach (var formation in troop)
                {
                    Formation targetFormation;
                    if (!data.TryGetValue(formation.Type, out targetFormation))
                        return false;

                    foreach (var unit in formation)
                        targetFormation.Remove(unit.Key, unit.Value);
                }

                FireUpdated();
            }

            return true;
        }

        public ushort RemoveUnit(FormationType formationType, ushort type, ushort count)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                Formation formation;
                if (data.TryGetValue(formationType, out formation))
                {
                    ushort removed = formation.Remove(type, count);
                    if (removed > 0)
                    {
                        FireUpdated();
                        return removed;
                    }
                }

                return 0;
            }
        }

        public void RemoveAllUnits()
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (var formation in data.Values)
                    formation.Clear();

                FireUpdated();
            }
        }

        public void RemoveAllUnits(params FormationType[] formations)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (var formation in data.Values)
                {
                    if (!formations.Contains(formation.Type))
                        continue;

                    formation.Clear();
                }

                FireUpdated();
            }
        }

        public bool HasEnough(TroopStub troop)
        {
            lock (objLock)
            {
                foreach (var formation in troop)
                {
                    Formation targetFormation;
                    if (!data.TryGetValue(formation.Type, out targetFormation))
                        return false;

                    foreach (var unit in formation)
                    {
                        ushort count;

                        if (!targetFormation.TryGetValue(unit.Key, out count) || count < unit.Value)
                            return false;
                    }
                }
            }
            return true;
        }

        public void Print()
        {
            Dictionary<FormationType, Formation>.Enumerator itr = data.GetEnumerator();
            while (itr.MoveNext())
            {
                Global.Logger.Info(string.Format("Formation type: " + Enum.GetName(typeof(FormationType), itr.Current.Key)));
                Dictionary<ushort, ushort>.Enumerator itr2 = itr.Current.Value.GetEnumerator();
                while (itr2.MoveNext())
                    Global.Logger.Error(string.Format("\t\tType[{0}] : Count[{1}]", itr2.Current.Key, itr2.Current.Value));
            }
        }

        public bool TryGetValue(FormationType formationType, out Formation formation)
        {
            return data.TryGetValue(formationType, out formation);
        }

        public ushort GetFormationBits()
        {
            ushort mask = 0;
            foreach (var type in data.Keys)
                mask += (ushort)Math.Pow(2, (double)type);
            return mask;
        }
    }
}