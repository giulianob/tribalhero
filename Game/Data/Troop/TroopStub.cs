#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Database;
using Game.Logic.Actions;
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

        WaitingInDefensiveAssignment = 6,

        WaitingInOffensiveAssignment = 7
    }

    public enum TroopBattleGroup
    {
        Local = 0,

        Attack = 1,

        Defense = 2,

        Any = 3
    }

    public class TroopStub : SimpleStub, ITroopStub
    {
        public const string DB_TABLE = "troop_stubs";

        private readonly object objLock = new object();

        private bool isDirty;

        private bool isUnitDirty;

        private bool isUpdating;

        #region Events

        public delegate void OnUpdate(TroopStub stub);

        public delegate void Removed(ITroopStub stub);

        public delegate void StateSwitched(ITroopStub stub, TroopState newState);

        public event StateSwitched OnStateSwitched = delegate { };

        public event Removed OnRemoved = delegate { };

        public event OnUpdate Update = delegate { };

        public event OnUpdate UnitUpdate = delegate { };

        #endregion

        #region Properties

        private ushort initialCount;

        private ushort retreatCount;

        private AttackMode attackMode;

        private TroopState state = TroopState.Idle;

        private IStation station;

        private byte troopId;

        //private ITroopObject troopObject;
        public TroopTemplate Template { get; private set; }

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

        public ICity City { get; set; }

        public byte StationTroopId { get; set; }

        public IStation Station
        {
            get
            {
                return station;
            }
            set
            {
                CheckUpdateMode();
                station = value;
            }
        }

        public ushort InitialCount
        {
            get
            {
                return initialCount;
            }
            set
            {
                CheckUpdateMode();
                initialCount = value;
            }
        }

        public ushort RetreatCount
        {
            get
            {
                return retreatCount;
            }
            set
            {
                CheckUpdateMode();
                retreatCount = value;
            }
        }

        public AttackMode AttackMode
        {
            get
            {
                return attackMode;
            }
            set
            {
                CheckUpdateMode();
                attackMode = value;
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
                        {
                            count += (kvp.Value * Template[kvp.Key].MaxHp);
                        }
                    }
                }

                return count;
            }
        }

        public decimal Speed
        {
            get
            {
                return Formula.Current.GetTroopSpeed(this);
            }
        }

        /// <summary>
        ///     Returns the sum of the upkeep for all units in troop stub
        /// </summary>
        public int Value
        {
            get
            {
                int count = 0;

                lock (objLock)
                {
                    foreach (var formation in data.Values)
                    {
                        count += formation.Sum(kvp => City.Template[kvp.Key].Upkeep * kvp.Value);
                    }
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
                    count += data.Values.Sum(formation => UpkeepForFormation(formation.Type));
                }

                return count;
            }
        }

        public int UpkeepForFormation(FormationType type)
        {
            Formation formation;
            if (!TryGetValue(type, out formation))
            {
                return 0;
            }

            return formation.Sum(kvp => kvp.Value * City.Template[kvp.Key].Upkeep);
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
                        {
                            count += (kvp.Value * Template[kvp.Key].Carry);
                        }
                    }
                }

                return count;
            }
        }

        public void Starve(int percent = 5, bool bypassProtection = false)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (var formation in data.Values)
                {
                    if (!bypassProtection && formation.Values.Sum(x => x) <= 1)
                    {
                        continue;
                    }
                    foreach (var kvp in new Dictionary<ushort, ushort>(formation))
                    {
                        var newCount = (ushort)(kvp.Value * (100 - percent) / 100);

                        if (newCount == 0)
                        {
                            formation.Remove(kvp.Key);
                        }
                        else
                        {
                            formation[kvp.Key] = newCount;
                        }
                    }
                }

                FireUnitUpdated();
            }
        }

        public bool IsDefault()
        {
            return TroopId == 1;
        }

        #endregion

        public TroopStub(byte troopId, ICity city)
        {
            City = city;
            this.troopId = troopId;
            Template = new TroopTemplate(this);
        }

        #region IEnumerable<Formation> Members

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
                return new[]
                {
                        new DbColumn("id", troopId, DbType.UInt32), 
                        new DbColumn("city_id", City.Id, DbType.UInt32)
                };
            }
        }

        public IEnumerable<DbDependency> DbDependencies
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
                        new DbColumn("station_type", station != null ? station.LocationType : 0, DbType.Int32),
                        new DbColumn("station_id", station != null ? station.LocationId : 0, DbType.Int32),
                        new DbColumn("state", (byte)state, DbType.Byte),
                        new DbColumn("formations", GetFormationBits(), DbType.UInt16),
                        new DbColumn("retreat_count", retreatCount, DbType.UInt16),
                        new DbColumn("initial_count", initialCount, DbType.UInt16),
                        new DbColumn("attack_mode", (byte)attackMode, DbType.Byte)
                };
            }
        }

        public IEnumerable<DbColumn> DbListColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("formation_type", DbType.Byte), new DbColumn("type", DbType.UInt16),
                        new DbColumn("count", DbType.UInt16)
                };
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
                                        new DbColumn("formation_type", (int)itr.Current.Key, DbType.Byte),
                                        new DbColumn("type", itr2.Current.Key, DbType.UInt16),
                                        new DbColumn("count", itr2.Current.Value, DbType.UInt16)
                                };
                    }
                }
            }
        }

        #endregion

        public void FireUnitUpdated()
        {
            if (!Global.FireEvents)
            {
                return;
            }

            CheckUpdateMode();

            isUnitDirty = true;
        }

        public void FireUpdated()
        {
            if (!Global.FireEvents)
            {
                return;
            }

            CheckUpdateMode();

            isDirty = true;
        }

        public void BeginUpdate()
        {
            if (isUpdating)
            {
                throw new Exception("Nesting beginupdates");
            }

            isUpdating = true;
            isDirty = false;
            isUnitDirty = false;
        }

        public void EndUpdate()
        {
            isUpdating = false;

            DbPersistance.Current.Save(this);

            if (isDirty || isUnitDirty)
            {
                Update(this);
            }

            if (isUnitDirty)
            {
                UnitUpdate(this);
            }
        }

        public void RemoveFormation(FormationType type)
        {
            data[type].OnUnitUpdated -= FormationOnUnitUpdated;
            data.Remove(type);
        }

        public bool AddFormation(FormationType type)
        {
            lock (objLock)
            {
                CheckUpdateMode();
                if (data.ContainsKey(type))
                {
                    return false;
                }
                var formation = new Formation(type);
                formation.OnUnitUpdated += FormationOnUnitUpdated;
                data.Add(type, formation);

                FireUnitUpdated();
            }

            return true;
        }

        public void ChangeFormation(FormationType originalFormation, FormationType newFormation)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                if (!data.ContainsKey(originalFormation))
                {
                    throw new Exception("Trying to move from invalid formation");
                }

                if (!AddFormation(newFormation))
                {
                    throw new Exception("New formation already exists");
                }

                foreach (var unit in data[originalFormation])
                {
                    AddUnit(newFormation, unit.Key, unit.Value);
                }

                RemoveFormation(originalFormation);

                FireUnitUpdated();
            }
        }

        public void Add(ISimpleStub stub)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                foreach (Formation stubFormation in stub)
                {
                    Formation targetFormation;
                    if (!data.TryGetValue(stubFormation.Type, out targetFormation))
                    {
                        targetFormation = new Formation(stubFormation.Type);
                        targetFormation.OnUnitUpdated += FormationOnUnitUpdated;
                        data.Add(stubFormation.Type, targetFormation);
                    }

                    targetFormation.Add(stubFormation);
                }

                FireUnitUpdated();
            }
        }

        public override void AddUnit(FormationType formationType, ushort type, ushort count)
        {
            lock (objLock)
            {
                Formation formation;
                if (data.TryGetValue(formationType, out formation))
                {
                    formation.Add(type, count);
                }

                FireUnitUpdated();
            }
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
                        FireUnitUpdated();
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
                    {
                        continue;
                    }

                    formation.Clear();
                }

                FireUnitUpdated();
            }
        }

        private void CheckUpdateMode(bool checkStationedCity = true)
        {
            if (!Global.FireEvents || City == null)
            {
                return;
            }

            if (!isUpdating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);

            if (checkStationedCity && station != null)
            {
                DefaultMultiObjectLock.ThrowExceptionIfNotLocked(station);
            }
        }

        private void FormationOnUnitUpdated()
        {
            FireUnitUpdated();
        }

        public bool Remove(ITroopStub troop)
        {
            lock (objLock)
            {
                CheckUpdateMode();

                if (!HasEnough(troop))
                {
                    return false;
                }

                foreach (Formation formation in troop)
                {
                    Formation targetFormation;
                    if (!data.TryGetValue(formation.Type, out targetFormation))
                    {
                        return false;
                    }

                    foreach (var unit in formation)
                    {
                        targetFormation.Remove(unit.Key, unit.Value);
                    }
                }

                FireUnitUpdated();
            }

            return true;
        }

        public bool HasEnough(ITroopStub troop)
        {
            lock (objLock)
            {
                foreach (var formation in troop)
                {
                    Formation targetFormation;
                    if (!data.TryGetValue(formation.Type, out targetFormation))
                    {
                        return false;
                    }

                    foreach (var unit in formation)
                    {
                        ushort count;

                        if (!targetFormation.TryGetValue(unit.Key, out count) || count < unit.Value)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public void Print()
        {
            var logger = LoggerFactory.Current.GetCurrentClassLogger();
            Dictionary<FormationType, Formation>.Enumerator itr = data.GetEnumerator();
            while (itr.MoveNext())
            {
                logger.Info(
                                   string.Format("Formation type: " +
                                                 Enum.GetName(typeof(FormationType), itr.Current.Key)));
                Dictionary<ushort, ushort>.Enumerator itr2 = itr.Current.Value.GetEnumerator();
                while (itr2.MoveNext())
                {
                    logger.Error(string.Format("\t\tType[{0}] : Count[{1}]", itr2.Current.Key, itr2.Current.Value));
                }
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
            {
                mask += (ushort)Math.Pow(2, (double)type);
            }
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
    }
}