  using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Game.Data;
using Game.Database;

namespace Game.Util {
    public interface ILockable {
        int Hash { get; }
        object Lock { get; }
    }

    public class MultiObjectLock : IDisposable {

        [ThreadStatic]
        static MultiObjectLock currentLock = null;

        DbTransaction transaction = null;

        private static int CompareObject(ILockable x, ILockable y) {
            return x.Hash.CompareTo(y.Hash);
        }

        void Lock(params ILockable[] list) {
            lockedObjects = new object[list.Length];

            if (currentLock != null) {
                throw new Exception("Attempting to nest MultiObjectLock");
            }

            currentLock = this;

            Array.Sort<ILockable>(list, CompareObject);
            for (int i = 0; i < list.Length; ++i)
            {
                Monitor.Enter(list[i].Lock);
                lockedObjects[i] = list[i].Lock;
            }

            transaction = Global.dbManager.GetThreadTransaction();
        }

        void UnlockAll() {
            transaction.Dispose();

            for (int i = lockedObjects.Length - 1; i >= 0; --i)
                Monitor.Exit(lockedObjects[i]);
            
            lockedObjects = new object[] { };

            currentLock = null;
        }

        object[] lockedObjects = new object[] { };

        public MultiObjectLock(params ILockable[] list) {
            Lock(list);
        }

        public MultiObjectLock(out Dictionary<uint, City> result, params uint[] cityIds) {
            result = new Dictionary<uint,City>(cityIds.Length);
            
            City[] cities = new City[cityIds.Length];

            int i = 0;
            foreach (uint cityId in cityIds) {
                City city;
                if (!Global.World.TryGetObjects(cityId, out city)) {
                    result = null;
                    return;
                }

                result[cityId] = city;
                cities[i++] = city;
            }

            Lock(cities);
        }

        public MultiObjectLock(uint cityId, out City city) {
            city = null;

            TryGetCity(cityId, out city);
        }

        public MultiObjectLock(uint cityId, uint objectId, out City city, out Structure obj) {
            city = null;
            obj = null;

            TryGetCityStructure(cityId, objectId, out city, out obj);
        }

        public MultiObjectLock(uint cityId, uint objectId, out City city, out TroopObject obj) {
            city = null;
            obj = null;

            TryGetCityTroop(cityId, objectId, out city, out obj);
        }

        bool TryGetCity(uint cityId, out City city) {
            city = null;

            if (!Global.World.TryGetObjects(cityId, out city)) {
                return false;
            }
            
            try {
                Lock(city);
            }
            catch (Exception) {
                city = null;
                return false;
            }

            return true;
        }

        bool TryGetCityStructure(uint cityId, uint objectId, out City city, out Structure obj) {
            city = null;
            obj = null;

            if (!TryGetCity(cityId, out city)) {
                return false;
            }

            if (!city.tryGetStructure(objectId, out obj)) {
                city = null;
                obj = null;
                UnlockAll();
                return false;
            }

            return true;
        }

        bool TryGetCityTroop(uint cityId, uint objectId, out City city, out TroopObject obj) {
            city = null;
            obj = null;

            if (!TryGetCity(cityId, out city)) {
                return false;
            }

            if (!city.tryGetTroop(objectId, out obj)) {
                city = null;
                obj = null;
                UnlockAll();
                return false;
            }

            return true;
        }

        public void Dispose() {            
            UnlockAll();            
        }
    }
}
