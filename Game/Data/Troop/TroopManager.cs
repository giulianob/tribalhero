#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Database;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Data.Troop
{
    public class TroopManager : ITroopManager
    {
        public ITroopStubFactory TroopStubFactory { get; set; }

        #region Event

        #region Delegates

        public delegate void UpdateCallback(ITroopStub stub);

        #endregion

        public event UpdateCallback TroopUpdated;

        public event UpdateCallback TroopAdded;

        public event UpdateCallback TroopRemoved;

        #endregion

        #region Properties

        private readonly Dictionary<byte, ITroopStub> dict = new Dictionary<byte, ITroopStub>();

        private readonly SmallIdGenerator idGen = new SmallIdGenerator(byte.MaxValue, true);

        public byte Size
        {
            get
            {
                return (byte)dict.Count;
            }
        }

        public IStation BaseStation { get; set; }

        public ITroopStub this[byte index]
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

        #endregion

        #region Methods

        public TroopManager(IStation baseStation = null, ITroopStubFactory troopStubFactory = null)
        {
            TroopStubFactory = troopStubFactory;
            BaseStation = baseStation;
        }

        public int Upkeep
        {
            get
            {
                return MyStubs().Sum(stub => stub.Upkeep);
            }
        }

        public bool DbLoaderAdd(byte id, ITroopStub stub)
        {
            if (dict.ContainsKey(id))
            {
                return false;
            }
            idGen.Set(id);
            dict[id] = stub;
            stub.UnitUpdate += StubUpdateEvent;
            return true;
        }

        public bool DbLoaderAddStation(ITroopStub stub)
        {
            int nextId = idGen.GetNext();
            if (nextId == -1)
            {
                return false;
            }
            var id = (byte)nextId;

            stub.StationTroopId = id;
            stub.Station = BaseStation;

            dict.Add(id, stub);
            stub.UnitUpdate += StubUpdateEvent;
            FireAdded(stub);
            return true;
        }

        public ITroopStub Create()
        {
            var stub = TroopStubFactory.CreateTroopStub((byte)idGen.GetNext());
            dict.Add(stub.TroopId, stub);
            stub.UnitUpdate += StubUpdateEvent;
            FireAdded(stub);
            return stub;
        }

        public bool Add(ITroopStub stub, out byte id)
        {
            int nextId = idGen.GetNext();

            if (nextId == -1)
            {
                id = 0;
                return false;
            }

            id = (byte)nextId;

            stub.BeginUpdate();
            stub.TroopId = id;
            stub.EndUpdate();
                    // Updating inside prevents Endupate being called at higher level, causing an "Update" to be sent after "Add"

            dict.Add(id, stub);

            stub.UnitUpdate += StubUpdateEvent;

            FireAdded(stub);
            return true;
        }

        public bool AddStationed(ITroopStub stub)
        {
            int nextId = idGen.GetNext();
            if (nextId == -1)
            {
                return false;
            }
            var id = (byte)nextId;

            stub.BeginUpdate();
            stub.StationTroopId = id;
            stub.State = TroopState.Stationed;
            stub.Station = BaseStation;
            stub.EndUpdate();

            dict.Add(id, stub);
            stub.UnitUpdate += StubUpdateEvent;
            FireAdded(stub);
            return true;
        }

        public bool Add(ITroopStub stub)
        {
            byte id;
            return Add(stub, out id);
        }

        public bool RemoveStationed(byte id)
        {
            ITroopStub stub;
            if (!dict.TryGetValue(id, out stub))
            {
                return false;
            }
            if (!dict.Remove(id))
            {
                return false;
            }
            idGen.Release(id);

            stub.BeginUpdate();
            stub.StationTroopId = 0;
            stub.Station = null;
            stub.EndUpdate();

            stub.UnitUpdate -= StubUpdateEvent;

            FireRemoved(stub);
            return true;
        }

        public bool Remove(byte id)
        {
            ITroopStub stub;

            if (!dict.TryGetValue(id, out stub))
            {
                return false;
            }

            if (!dict.Remove(id))
            {
                return false;
            }

            stub.FireRemoved();

            idGen.Release(id);
            stub.UnitUpdate -= StubUpdateEvent;

            FireRemoved(stub);
            return true;
        }

        public bool TryGetStub(byte id, out ITroopStub stub)
        {
            return dict.TryGetValue(id, out stub);
        }

        public void Starve(int percent = 5, bool bypassProtection = false)
        {
            // Make a copy of the stub list since it might change during the foreach loop
            var troopStubs = new TroopStub[dict.Values.Count];
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
            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(BaseStation);
        }

        private void FireUpdated(TroopStub stub)
        {
            if (!Global.FireEvents)
            {
                return;
            }

            CheckUpdateMode();

            if (TroopUpdated != null)
            {
                TroopUpdated(stub);
            }
        }

        private void FireAdded(ITroopStub stub)
        {
            if (!Global.FireEvents)
            {
                return;
            }

            CheckUpdateMode();

            if (TroopAdded != null)
            {
                TroopAdded(stub);
            }
        }

        private void FireRemoved(ITroopStub stub)
        {
            if (!Global.FireEvents)
            {
                return;
            }

            CheckUpdateMode();

            //We don't want to delete a troopstub that doesn't belong to us.
            if (stub.City == BaseStation)
            {
                DbPersistance.Current.Delete(stub);
            }

            if (TroopRemoved != null)
            {
                TroopRemoved(stub);
            }
        }

        #endregion

        #region Callbacks

        public void StubUpdateEvent(TroopStub stub)
        {
            FireUpdated(stub);
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