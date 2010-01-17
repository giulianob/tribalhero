#region

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

        private City city;

        public City City {
            get { return city; }
            set { city = value; }
        }

        #endregion

        #region Methods

        public TroopManager(City city) {
            this.city = city;
        }

        private void FireUpdated(TroopStub stub) {
            if (!Global.FireEvents)
                return;

            if (stub.City.Id > 0)
                Global.dbManager.Save(stub);

            if (TroopUpdated != null)
                TroopUpdated(stub);
        }

        private void FireAdded(TroopStub stub) {
            if (!Global.FireEvents)
                return;

            if (TroopAdded != null)
                TroopAdded(stub);
        }

        private void FireRemoved(TroopStub stub) {
            if (!Global.FireEvents)
                return;

            //We don't want to delete a troopstub that doesn't belong to us.
            if (stub.City == city)
                Global.dbManager.Delete(stub);

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
            id = 0;
            int nextId = idGen.getNext();
            if (nextId == -1)
                return false;
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
            stub.State = TroopStub.TroopState.STATIONED;
            stub.StationedCity = city;
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

        public Resource Upkeep() {
            int crop = 0;
            foreach (TroopStub stub in dict.Values)
                crop += stub.Upkeep;
            return new Resource(crop, 0, 0, 0, 0);
        }

        public void Starve() {
            foreach (TroopStub stub in dict.Values) {
                stub.BeginUpdate();
                stub.Starve();
                stub.EndUpdate();
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

        internal IEnumerable<TroopStub> StationedHere() {
            foreach (TroopStub stub in this) {
                if (stub.StationedCity == city)
                    yield return stub;
            }
        }
    }
}