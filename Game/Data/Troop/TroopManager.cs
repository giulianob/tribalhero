#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Util;
using Persistance;
using Game.Util.Locking;

#endregion

namespace Game.Data.Troop
{
    public class TroopManager : ITroopManager
    {
        private readonly IDbManager dbManager;

        #region Event

        #region Delegates

        public delegate void UpdateCallback(ITroopStub stub);

        #endregion

        public event UpdateCallback TroopUnitUpdated = delegate { };

        public event UpdateCallback TroopUpdated = delegate { };

        public event UpdateCallback TroopAdded = delegate { };

        public event UpdateCallback TroopRemoved = delegate { };

        #endregion

        #region Properties

        private readonly Dictionary<ushort, ITroopStub> dict = new Dictionary<ushort, ITroopStub>();

        private readonly SmallIdGenerator idGen = new SmallIdGenerator(ushort.MaxValue, true);

        public ushort Size
        {
            get
            {
                return (ushort)dict.Count;
            }
        }

        public IStation BaseStation { get; set; }

        #endregion

        #region Methods

        public TroopManager(IDbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public ITroopStub this[ushort index]
        {
            get
            {
                return dict[index];
            }
            set
            {
                dict[index] = value;
            }
        }

        public SmallIdGenerator IdGen
        {
            get
            {
                return idGen;
            }
        }

        private void DeregisterStub(ushort id, ITroopStub stub)
        {
            if (!dict.ContainsKey(id))
            {
                return;
            }

            idGen.Release(id);
            dict.Remove(id);
            stub.Update -= StubUpdateEvent;
            stub.UnitUpdate -= StubUnitUpdateEvent;
            FireRemoved(stub);
        }

        public int Upkeep
        {
            get
            {
                return MyStubs().Sum(stub => stub.Upkeep);
            }
        }

        private void RegisterStub(ushort id, ITroopStub stub)
        {
            if (dict.ContainsKey(id))
            {
                return;
            }

            dict.Add(id, stub);
            stub.Update += StubUpdateEvent;
            stub.UnitUpdate += StubUnitUpdateEvent;
            FireAdded(stub);
        }

        public bool AddStationed(ITroopStub stub)
        {
            if (BaseStation == null)
            {
                throw new Exception("Cannot station in this troop manager");
            }

            int nextId = IdGen.GetNext();

            if (nextId == -1)
            {
                return false;
            }

            var stationTroopId = (ushort)nextId;

            stub.BeginUpdate();
            stub.StationTroopId = stationTroopId;
            stub.State = TroopState.Stationed;
            stub.Station = BaseStation;
            stub.EndUpdate();

            RegisterStub(stationTroopId, stub);
            return true;
        }

        public void DbLoaderAddStation(ITroopStub stub)
        {
            if (BaseStation == null)
            {
                throw new Exception("Cannot station in this troop manager");
            }

            int nextId = IdGen.GetNext();
            if (nextId == -1)
            {
                return;
            }

            var stationTroopId = (ushort)nextId;

            stub.StationTroopId = stationTroopId;
            stub.Station = BaseStation;

            RegisterStub(stationTroopId, stub);                     
        }

        public void Add(ITroopStub stub)
        {
            idGen.Set(stub.TroopId);

            RegisterStub(stub.TroopId, stub);            
        }

        public bool RemoveStationed(ushort stationTroopId)
        {
            ITroopStub stub;
            if (!dict.TryGetValue(stationTroopId, out stub))
            {
                return false;
            }

            if (stub.StationTroopId != stationTroopId)
            {
                throw new Exception("Trying to unstation one of the local troops");
            }

            stub.BeginUpdate();
            stub.StationTroopId = 0;
            stub.Station = null;
            stub.EndUpdate();

            DeregisterStub(stationTroopId, stub);
            return true;
        }

        public bool Remove(ushort id)
        {
            ITroopStub stub;

            if (!dict.TryGetValue(id, out stub))
            {
                return false;
            }
            
            if (stub.City != BaseStation)
            {
                throw new Exception("Trying to remove invalid troop");
            }

            DeregisterStub(id, stub);
            dbManager.Delete(stub);

            return true;
        }

        public bool TryGetStub(ushort id, out ITroopStub stub)
        {
            return dict.TryGetValue(id, out stub);
        }

        public void Starve(int percent = 5, bool bypassProtection = false)
        {
            // Make a copy of the stub list since it might change during the foreach loop
            var troopStubs = new ITroopStub[dict.Values.Count];
            dict.Values.CopyTo(troopStubs, 0);

            foreach (var stub in troopStubs)
            {
                // Skip troops that aren't ours
                if (stub.City != BaseStation)
                {
                    continue;
                }

                // Skip troops that are in battle
                if (stub.State == TroopState.Battle || stub.State == TroopState.BattleStationed)
                {
                    continue;
                }

                // Starve the troop
                stub.BeginUpdate();
                stub.Starve(percent, bypassProtection);
                stub.EndUpdate();

                // Remove it if it's been starved to death (and isn't the default troop)
                if (stub.TotalCount == 0 && !stub.IsDefault())
                {
                    if (stub.Station != null)
                    {
                        stub.Station.Troops.RemoveStationed(stub.StationTroopId);
                    }

                    Remove(stub.TroopId);
                }
            }
        }

        private void CheckUpdateMode()
        {
            if (!Global.Current.FireEvents)
            {
                return;
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(BaseStation);
        }

        private void FireUpdated(TroopStub stub)
        {
            CheckUpdateMode();

            if (TroopUpdated != null)
            {
                TroopUpdated(stub);
            }
        }

        private void FireUnitUpdated(TroopStub stub)
        {
            CheckUpdateMode();

            TroopUnitUpdated(stub);
        }

        private void FireAdded(ITroopStub stub)
        {
            CheckUpdateMode();

            TroopAdded(stub);            
        }

        private void FireRemoved(ITroopStub stub)
        {
            CheckUpdateMode();
            
            TroopRemoved(stub);            
        }

        #endregion

        #region Callbacks

        private void StubUpdateEvent(TroopStub stub)
        {
            FireUpdated(stub);
        }

        private void StubUnitUpdateEvent(TroopStub stub)
        {
            FireUnitUpdated(stub);
        }

        #endregion

        #region IEnumerable<TroopStub> Members

        public IEnumerator<ITroopStub> GetEnumerator()
        {
            return dict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.Values.GetEnumerator();
        }

        #endregion

        #region Enumeration Helpers

        /// <summary>
        ///     Returns all foreign troops that are stationed in this city.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITroopStub> StationedHere()
        {
            return this.Where(stub => stub.Station == BaseStation);
        }

        /// <summary>
        ///     Only returns troops that belong to this city. Won't return troops that are stationed here but will return troops that may be stationed outside of the city.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITroopStub> MyStubs()
        {
            return this.Where(stub => stub.City == BaseStation);
        }

        #endregion
    }
}