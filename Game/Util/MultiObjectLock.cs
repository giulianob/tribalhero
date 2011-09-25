#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Data;
using Game.Data.Troop;
using Game.Data.Tribe;
using Game.Setup;
using Ninject;
using Persistance;

#endregion

namespace Game.Util
{
    public interface ILockable
    {
        int Hash { get; }
        object Lock { get; }
    }

    public class LockException : Exception
    {
        public LockException(string message) : base(message)
        {
        }
    }

    public class MultiObjectLock : IDisposable
    {
        [ThreadStatic]
        private static MultiObjectLock currentLock;

        private object[] lockedObjects = new object[] {};
        private DbTransaction transaction;

        public MultiObjectLock(params ILockable[] list)
        {
            Lock(list);
        }

        public MultiObjectLock(out Dictionary<uint, City> result, params uint[] cityIds)
        {
            result = new Dictionary<uint, City>(cityIds.Length);

            var cities = new City[cityIds.Length];

            int i = 0;
            foreach (var cityId in cityIds)
            {
                City city;
                if (!Global.World.TryGetObjects(cityId, out city))
                {
                    result = null;
                    return;
                }

                result[cityId] = city;
                cities[i++] = city;
            }

            Lock(cities);
        }

        public MultiObjectLock(out Dictionary<uint, Player> result, params uint[] playerIds) {
            result = new Dictionary<uint, Player>(playerIds.Length);
            var players = new Player[playerIds.Length];

            int i = 0;
            foreach (var playerId in playerIds) {
                Player player;
                if (!Global.World.TryGetObjects(playerId, out player)) {
                    result = null;
                    return;
                }

                result[playerId] = player;
                players[i++] = player;
            }

            Lock(players);
        }

        public MultiObjectLock(uint tribeId, out Tribe tribe) {
            TryGetTribe(tribeId, out tribe);
        }
        
        public MultiObjectLock(uint playerId, out Player player)
        {
            TryGetPlayer(playerId, out player);
        }

        public MultiObjectLock(uint playerId, out Player player, out Tribe tribe)
        {
            if (!Global.World.TryGetObjects(playerId, out player))
            {
                tribe = null;
                return;
            }

            if (player.Tribesman == null)
            {
                tribe = null;
                return;
            }
           
            try {
                if (!player.Tribesman.Tribe.IsOwner(player)) {
                    Lock(player,player.Tribesman.Tribe);
                } 
                else
                {
                    Lock(player);
                }
                tribe = player.Tribesman.Tribe;
            } catch (LockException) {
                throw;
            } catch (Exception) {
                tribe = null;
            }

        }

        public MultiObjectLock(uint cityId, out City city)
        {
            TryGetCity(cityId, out city);
        }

        public MultiObjectLock(uint cityId, uint objectId, out City city, out Structure obj)
        {
            TryGetCityStructure(cityId, objectId, out city, out obj);
        }

        public MultiObjectLock(uint cityId, uint objectId, out City city, out TroopObject obj)
        {
            TryGetCityTroop(cityId, objectId, out city, out obj);
        }

        #region IDisposable Members

        public void Dispose()
        {
            UnlockAll();
        }

        #endregion

        public static void ThrowExceptionIfNotLocked(ILockable obj)
        {
#if DEBUG
            if (!IsLocked(obj))
                throw new LockException("Object not locked");

#elif CHECK_LOCKS
            if (!IsLocked(obj)) 
                Global.Logger.Error(string.Format("Object not locked id[{0}] {1}", obj.Hash, Environment.StackTrace));
#endif
        }

        public static bool IsLocked(ILockable obj)
        {
            return currentLock != null && currentLock.lockedObjects.Any(lck => lck == obj.Lock);
        }

        private static int CompareObject(ILockable x, ILockable y)
        {
            return x.Hash.CompareTo(y.Hash);
        }

        private void Lock(params ILockable[] list)
        {
            lockedObjects = new object[list.Length];

            if (currentLock != null)
                throw new LockException("Attempting to nest MultiObjectLock");

            currentLock = this;

            Array.Sort(list, CompareObject);
            for (int i = 0; i < list.Length; ++i)
            {
                Monitor.Enter(list[i].Lock);
                lockedObjects[i] = list[i].Lock;
            }

            transaction = Ioc.Kernel.Get<IDbManager>().GetThreadTransaction();
        }

        private void UnlockAll()
        {
            if (transaction != null)
                transaction.Dispose();

            for (int i = lockedObjects.Length - 1; i >= 0; --i)
                Monitor.Exit(lockedObjects[i]);

            lockedObjects = new object[] {};

            currentLock = null;
        }

        private bool TryGetCity(uint cityId, out City city)
        {
            if (!Global.World.TryGetObjects(cityId, out city))
                return false;

            try
            {
                Lock(city);
            }
            catch(LockException)
            {
                throw;
            }
            catch(Exception)
            {
                city = null;
                return false;
            }

            return true;
        }

        private bool TryGetPlayer(uint playerId, out Player player)
        {
            if (!Global.World.TryGetObjects(playerId, out player))
                return false;

            try
            {
                Lock(player);
            }
            catch(LockException)
            {
                throw;
            }
            catch(Exception)
            {
                player = null;
                return false;
            }

            return true;
        }

        private bool TryGetTribe(uint tribeId, out Tribe tribe) {
            if (!Global.Tribes.TryGetValue(tribeId, out tribe))
                return false;

            try {
                Lock(tribe);
            } catch (LockException) {
                throw;
            } catch (Exception) {
                tribe = null;
                return false;
            }

            return true;
        }

        private void TryGetCityStructure(uint cityId, uint objectId, out City city, out Structure obj)
        {
            obj = null;

            if (!TryGetCity(cityId, out city))
                return;

            if (city.TryGetStructure(objectId, out obj))
                return;

            city = null;
            obj = null;
            UnlockAll();
        }

        private void TryGetCityTroop(uint cityId, uint objectId, out City city, out TroopObject obj)
        {
            obj = null;

            if (!TryGetCity(cityId, out city))
                return;

            if (city.TryGetTroop(objectId, out obj))
                return;

            city = null;
            obj = null;
            UnlockAll();
        }

    }

    public class CallbackLock : IDisposable
    {
        #region Delegates

        public delegate ILockable[] CallbackLockHandler(object[] custom);

        #endregion

        private readonly MultiObjectLock currentLock;

        public CallbackLock(CallbackLockHandler lockHandler, object[] lockHandlerParams, params ILockable[] baseLocks)
        {
            int count = 0;
            while (currentLock == null)
            {
                if ((++count)%5 == 0)
                    Global.Logger.Info(string.Format("CallbackLock has iterated {0} times from {1}", count, Environment.StackTrace));

                if (count >= 10000)
                    throw new LockException("Callback lock exceeded maximum count");

                var toBeLocked = new List<ILockable>(baseLocks);

                // Lock the base objects
                using (new MultiObjectLock(baseLocks))
                {
                    // Grab the list of objects we need to lock from the callback                    
                    toBeLocked.AddRange(lockHandler(lockHandlerParams));
                }

                // Lock all of the objects we believe should be locked
                currentLock = new MultiObjectLock(toBeLocked.ToArray());

                // Grab the current list of objects we need to lock from the callback
                var newToBeLocked = new List<ILockable>(baseLocks);
                newToBeLocked.AddRange(lockHandler(lockHandlerParams));

                // Make sure they are still all the same
                if (newToBeLocked.Count != toBeLocked.Count)
                {
                    currentLock.Dispose();
                    currentLock = null;
                    Thread.Sleep(0);
                    continue;
                }

                if (!newToBeLocked.Where((t, i) => t.Hash != toBeLocked[i].Hash).Any())
                    continue;

                currentLock.Dispose();
                currentLock = null;
                Thread.Sleep(0);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            currentLock.Dispose();
        }

        #endregion
    }
}