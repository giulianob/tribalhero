#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;

#endregion

namespace Game.Data.Troop
{
    public class TroopManager : IEnumerable<TroopStub>
    {
        #region Event

        #region Delegates

        public delegate void UpdateCallback(TroopStub stub);

        #endregion

        public event UpdateCallback TroopUpdated;
        public event UpdateCallback TroopAdded;
        public event UpdateCallback TroopRemoved;

        #endregion

        #region Properties

        private readonly Dictionary<byte, TroopStub> dict = new Dictionary<byte, TroopStub>();
        private readonly SmallIdGenerator idGen = new SmallIdGenerator(byte.MaxValue, true);

        public byte Size
        {
            get
            {
                return (byte)dict.Count;
            }
        }

        public TroopStub this[byte index]
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

        public City City { get; set; }

        #endregion

        #region Methods

        public TroopManager(City city)
        {
            City = city;
        }

        public int Upkeep
        {
            get
            {
                return MyStubs().Sum(stub => stub.Upkeep);
            }
        }

        private void CheckUpdateMode()
        {
            MultiObjectLock.ThrowExceptionIfNotLocked(City);
        }

        private void FireUpdated(TroopStub stub)
        {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            Ioc.Kernel.Get<IDbManager>().Save(stub);

            if (TroopUpdated != null)
                TroopUpdated(stub);
        }

        private void FireAdded(TroopStub stub)
        {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            if (TroopAdded != null)
                TroopAdded(stub);
        }

        private void FireRemoved(TroopStub stub)
        {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            //We don't want to delete a troopstub that doesn't belong to us.
            if (stub.City == City)
                Ioc.Kernel.Get<IDbManager>().Delete(stub);

            if (TroopRemoved != null)
                TroopRemoved(stub);
        }

        public bool DbLoaderAdd(byte id, TroopStub stub)
        {
            if (dict.ContainsKey(id))
                return false;
            idGen.Set(id);
            dict[id] = stub;
            stub.BeginUpdate();
            stub.TroopManager = this;
            stub.EndUpdate();
            stub.UnitUpdate += StubUpdateEvent;
            return true;
        }

        public bool Add(TroopStub stub, out byte id)
        {
            int nextId = idGen.GetNext();

            if (nextId == -1)
            {
                id = 0;
                return false;
            }

            id = (byte)nextId;

            stub.TroopId = id;
            stub.TroopManager = this;

            dict.Add(id, stub);

            stub.UnitUpdate += StubUpdateEvent;

            FireAdded(stub);
            return true;
        }

        public bool AddStationed(TroopStub stub)
        {
            int nextId = idGen.GetNext();
            if (nextId == -1)
                return false;
            var id = (byte)nextId;
            stub.StationedTroopId = id;
            stub.State = TroopState.Stationed;
            stub.StationedCity = City;
            dict.Add(id, stub);
            stub.UnitUpdate += StubUpdateEvent;
            FireAdded(stub);
            return true;
        }

        public bool Add(TroopStub stub)
        {
            byte id;
            return Add(stub, out id);
        }

        public bool RemoveStationed(byte id)
        {
            TroopStub stub;
            if (!dict.TryGetValue(id, out stub))
                return false;
            if (!dict.Remove(id))
                return false;
            idGen.Release(id);

            stub.BeginUpdate();
            stub.StationedTroopId = 0;
            stub.StationedCity = null;
            stub.EndUpdate();

            stub.UnitUpdate -= StubUpdateEvent;

            FireRemoved(stub);
            return true;
        }

        public bool Remove(byte id)
        {
            TroopStub stub;

            if (!dict.TryGetValue(id, out stub))
                return false;

            if (!dict.Remove(id))
                return false;

            idGen.Release(id);
            stub.UnitUpdate -= StubUpdateEvent;

            FireRemoved(stub);
            return true;
        }

        public bool TryGetStub(byte id, out TroopStub stub)
        {
            return dict.TryGetValue(id, out stub);
        }

        public void Starve(int percent = 5, bool bypassProtection=false)
        {
            // Make a copy of the stub list since it might change during the foreach loop
            var troopStubs = new TroopStub[dict.Values.Count];
            dict.Values.CopyTo(troopStubs, 0);

            foreach (var stub in troopStubs)
            {

                // Skip troops that aren't ours
                if (stub.City != City)
                    continue;

                // Skip troops that are in battle
                if (stub.State == TroopState.Battle || stub.State == TroopState.BattleStationed)
                    continue;

                // Starve the troop
                stub.BeginUpdate();
                stub.Starve(percent, bypassProtection);
                stub.EndUpdate();

                // Remove it if it's been starved to death (and isn't the default troop)
                if (stub.TotalCount == 0 && !stub.IsDefault())
                {
                    if (stub.StationedCity != null)
                    {
                        City stationedCity = stub.StationedCity;
                        stationedCity.Troops.RemoveStationed(stub.StationedTroopId);
                    }

                    City.Troops.Remove(stub.TroopId);
                }
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

        public IEnumerator<TroopStub> GetEnumerator()
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
        ///   Returns all foreign troops that are stationed in this city.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<TroopStub> StationedHere()
        {
            return this.Where(stub => stub.StationedCity == City);
        }

        /// <summary>
        ///   Only returns troops that belong to this city. Won't return troops that are stationed here but will return troops that may be stationed outside of the city.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<TroopStub> MyStubs()
        {
            return this.Where(stub => stub.City == City);
        }

        #endregion
    }
}