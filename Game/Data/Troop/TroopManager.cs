#region

using System;
using System.Collections;
using System.Collections.Generic;
using Game.Util;

#endregion

namespace Game.Data.Troop {
    public class TroopManager : IEnumerable<TroopStub> {
        #region Event

        public delegate void UpdateCallback(TroopStub stub);

        public event UpdateCallback TroopUpdated;
        public event UpdateCallback TroopAdded;
        public event UpdateCallback TroopRemoved;

        #endregion

        #region Properties

        private readonly SmallIdGenerator idGen = new SmallIdGenerator(byte.MaxValue, true);

        private readonly Dictionary<byte, TroopStub> dict = new Dictionary<byte, TroopStub>();

        public byte Size {
            get { return (byte) dict.Count; }
        }

        public TroopStub this[byte index] {
            get { return dict[index]; }
            set { dict[index] = value; }
        }

        public City City { get; set; }

        #endregion

        #region Methods

        public TroopManager(City city) {
            City = city;
        }
        
        private void CheckUpdateMode() {
            MultiObjectLock.ThrowExceptionIfNotLocked(City);
        }

        private void FireUpdated(TroopStub stub) {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            Global.DbManager.Save(stub);

            if (TroopUpdated != null)
                TroopUpdated(stub);
        }

        private void FireAdded(TroopStub stub) {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            if (TroopAdded != null)
                TroopAdded(stub);
        }

        private void FireRemoved(TroopStub stub) {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            //We don't want to delete a troopstub that doesn't belong to us.
            if (stub.City == City)
                Global.DbManager.Delete(stub);

            if (TroopRemoved != null)
                TroopRemoved(stub);
        }

        public bool DbLoaderAdd(byte id, TroopStub stub) {
            if (dict.ContainsKey(id))
                return false;
            idGen.set(id);
            dict[id] = stub;
            stub.BeginUpdate();
            stub.TroopManager = this;
            stub.EndUpdate();
            stub.UnitUpdate += StubUpdateEvent;
            return true;
        }

        public bool Add(TroopStub stub, out byte id) {            
            int nextId = idGen.getNext();

            if (nextId == -1) {
                id = 0;
                return false;
            }

            id = (byte) nextId;
            
            stub.TroopId = id;
            stub.TroopManager = this;
            
            dict.Add(id, stub);
            
            stub.UnitUpdate += StubUpdateEvent;

            FireAdded(stub);
            return true;
        }

        public bool AddStationed(TroopStub stub) {
            int nextId = idGen.getNext();
            if (nextId == -1)
                return false;
            byte id = (byte) nextId;
            stub.StationedTroopId = id;
            stub.State = TroopState.STATIONED;
            stub.StationedCity = City;
            dict.Add(id, stub);
            stub.UnitUpdate += StubUpdateEvent;
            FireAdded(stub);
            return true;
        }

        public bool Add(TroopStub stub) {
            byte id;
            return Add(stub, out id);
        }

        public bool RemoveStationed(byte id) {
            TroopStub stub;
            if (!dict.TryGetValue(id, out stub))
                return false;
            if (!dict.Remove(id))
                return false;
            idGen.release(id);

            stub.BeginUpdate();
            stub.StationedTroopId = 0;
            stub.StationedCity = null;
            stub.EndUpdate();

            stub.UnitUpdate -= StubUpdateEvent;

            FireRemoved(stub);
            return true;
        }

        public bool Remove(byte id) {
            TroopStub stub;

            if (id == 1) 
                throw new Exception("Trying to remove default troop");

            if (!dict.TryGetValue(id, out stub))
                return false;
            
            if (!dict.Remove(id))
                return false;
            
            idGen.release(id);
            stub.UnitUpdate -= StubUpdateEvent;
           
            FireRemoved(stub);
            return true;
        }

        public bool TryGetStub(byte id, out TroopStub stub) {
            return dict.TryGetValue(id, out stub);
        }

        public int Upkeep {
            get {
                int upkeep = 0;
                foreach (TroopStub stub in MyStubs()) {
                    upkeep += stub.Upkeep;
                }

                return upkeep;
            }
        }

        public void Starve() {
            // Make a copy of the stub list since it might change during the foreach loop
            TroopStub[] troopStubs = new TroopStub[dict.Values.Count];
            dict.Values.CopyTo(troopStubs, 0);

            foreach (TroopStub stub in troopStubs) {
                // Skip troops that aren't ours
                if (stub.City != City)
                    continue;

                // Skip troops that are on the move
                if (stub.TroopObject != null)
                    continue;

                // Skip troops that aren't idle
                if (stub.State != TroopState.STATIONED && stub.State != TroopState.IDLE)
                    continue;

                // Starve the troop
                stub.BeginUpdate();
                stub.Starve();
                stub.EndUpdate();

                // Remove it if it's been starved to death (and isn't the default troop)
                if (stub.TotalCount == 0 && !stub.IsDefault()) {
                    if (stub.StationedCity != null) {
                        City stationedCity = stub.StationedCity;
                        stationedCity.Troops.RemoveStationed(stub.StationedTroopId);
                    }

                    City.Troops.Remove(stub.TroopId);
                }
            }
        }

        #endregion

        #region Callbacks

        public void StubUpdateEvent(TroopStub stub) {
            FireUpdated(stub);
        }

        #endregion

        #region IEnumerable<TroopStub> Members

        public IEnumerator<TroopStub> GetEnumerator() {
            return dict.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return dict.Values.GetEnumerator();
        }

        #endregion

        #region Enumeration Helpers
        /// <summary>
        /// Returns all foreign troops that are stationed in this city.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<TroopStub> StationedHere() {
            foreach (TroopStub stub in this) {
                if (stub.StationedCity == City)
                    yield return stub;
            }
        }

        /// <summary>
        /// Only returns troops that belong to this city. Won't return troops that are stationed here but will return troops that may be stationed outside of the city.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<TroopStub> MyStubs() {
            foreach (TroopStub stub in this) {
                if (stub.City == City)
                    yield return stub;
            }
        }
        #endregion
    }
}