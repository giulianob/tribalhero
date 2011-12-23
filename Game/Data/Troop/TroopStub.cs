#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Logic.Formulas;
using Game.Util;
using Game.Util.Locking;
using Persistance;

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
        ReturningHome = 5,
        WaitingInAssignment = 6
    }

    public enum TroopBattleGroup
    {
        Local = 0,
        Attack = 1,
        Defense = 2,
    }

    public class TroopStub : ITroopStub
    {
        public const string DB_TABLE = "troop_stubs";
        private readonly object objLock = new object();
        protected Dictionary<FormationType, Formation> data = new Dictionary<FormationType, Formation>();
        private bool isDirty;
        private bool isUpdating;

        #region Events

        public delegate void StateSwitched(ITroopStub stub, TroopState newState);
        public event StateSwitched OnStateSwitched = delegate { };
        
        public delegate void Removed(ITroopStub stub);
        public event Removed OnRemoved = delegate { };

        public delegate void OnUnitUpdate(TroopStub stub);
        public event OnUnitUpdate UnitUpdate = delegate { };
        #endregion

        #region Properties

        private TroopState state = TroopState.Idle;
        private ICity stationedCity;
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
                OnStateSwitched(this, value);
            }
        }

        public ICity City
        {
            get
            {
                return TroopManager == null ? null : TroopManager.City;
            }
        }

        public byte StationedTroopId { get; set; }

        public ICity StationedCity
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

        public decimal TotalHp
        {
            get
            {
                decimal count = 0;

                lock (objLock)
                {
                    foreach (var formation in data.Values)
                    {
                        foreach (var kvp in formation)
                            count += (kvp.Value*Template[kvp.Key].MaxHp);
                    }
                }

                return count;
            }
        }

        public byte Speed
        {
            get
            {
                return Formula.Current.GetTroopSpeed(this);
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
                            count += (kvp.Value*Template[kvp.Key].Carry);
                    }
                }

                return count;
            }
        }

        public bool IsDefault()
        {
            return TroopId == 1;
        }

        public void Starve(int percent = 5, bool bypassProtection = false)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (var formation in data.Values)
                {
                    if (!bypassProtection && formation.Values.Sum(x => x) <= 1) continue;
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

        public IEnumerable<DbColumn[]> DbListValues()
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

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);

            if (checkStationedCity && stationedCity != null)
                DefaultMultiObjectLock.ThrowExceptionIfNotLocked(stationedCity);
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

        public bool Add(ITroopStub stub)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (Formation stubFormation in stub)
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

        public bool Remove(ITroopStub troop)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                if (!HasEnough(troop))
                    return false;

                foreach (Formation formation in troop)
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

        public bool HasFormation(FormationType formation)
        {
            return data.ContainsKey(formation);
        }

        public void FireRemoved()
        {
            OnRemoved(this);
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

        public void RemoveAllUnits(params FormationType[] formations)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (var formation in data.Values)
                {
                    if (formations != null && formations.Length > 0 && !formations.Contains(formation.Type))
                        continue;

                    formation.Clear();
                }

                FireUpdated();
            }
        }

        public bool HasEnough(ITroopStub troop)
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
        
        public void KeepFormations(params FormationType[] formations)
        {
            var currentFormations = data.Keys.ToList();

            foreach (FormationType formation in currentFormations.Where(formation => !formations.Contains(formation)))
            {
                data.Remove(formation);
            }
        }

        /// <summary>
        /// Returns a list of units for specified formations.
        /// If formation is empty, will return all units.
        /// </summary>
        /// <param name="formations"></param>
        /// <returns></returns>
        public List<Unit> ToUnitList(params FormationType[] formations)
        {
            var allUnits = from formation in data.Values
                           from unit in formation
                           where (formations.Length == 0 || formations.Contains(formation.Type))
                           orderby unit.Key
                           group unit by unit.Key
                           into unitGroups                            
                           select new Unit(unitGroups.Key, (ushort)unitGroups.Sum(x => x.Value));

            return allUnits.ToList();
        }
    }
}