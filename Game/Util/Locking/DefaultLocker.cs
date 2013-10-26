using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Map;

namespace Game.Util.Locking
{
    public class DefaultLocker : ILocker
    {
        private readonly CallbackLock.Factory callbackLockFactory;

        private readonly IGameObjectLocator locator;

        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory;

        public DefaultLocker(DefaultMultiObjectLock.Factory multiObjectLockFactory,
                             CallbackLock.Factory callbackLockFactory,
                             IGameObjectLocator locator)
        {
            this.multiObjectLockFactory = multiObjectLockFactory;
            this.callbackLockFactory = callbackLockFactory;
            this.locator = locator;
        }

        public IMultiObjectLock Lock(out Dictionary<uint, ICity> result, params uint[] cityIds)
        {
            result = new Dictionary<uint, ICity>(cityIds.Length);

            var cities = new ICity[cityIds.Length];

            int i = 0;
            foreach (var cityId in cityIds)
            {
                ICity city;
                if (!locator.TryGetObjects(cityId, out city))
                {
                    result = null;
                    return null;
                }

                result[cityId] = city;
                cities[i++] = city;
            }

            return Lock(cities);
        }

        public IMultiObjectLock Lock(out Dictionary<uint, IPlayer> result, params uint[] playerIds)
        {
            result = new Dictionary<uint, IPlayer>(playerIds.Length);
            var players = new ILockable[playerIds.Length];

            int i = 0;
            foreach (var playerId in playerIds)
            {
                IPlayer player;
                if (!locator.TryGetObjects(playerId, out player))
                {
                    result = null;
                    return null;
                }

                result[playerId] = player;
                players[i++] = player;
            }

            return Lock(players);
        }

        public IMultiObjectLock Lock(uint tribeId, out ITribe tribe)
        {
            return TryGetTribe(tribeId, out tribe);
        }

        public IMultiObjectLock Lock(uint playerId, out IPlayer player)
        {
            return TryGetPlayer(playerId, out player);
        }

        public IMultiObjectLock Lock(uint playerId, out IPlayer player, out ITribe tribe)
        {
            if (!locator.TryGetObjects(playerId, out player))
            {
                player = null;
                tribe = null;
                return null;
            }

            if (player.Tribesman == null)
            {
                player = null;
                tribe = null;
                return null;
            }

            try
            {
                tribe = player.Tribesman.Tribe;

                if (!player.Tribesman.Tribe.IsOwner(player))
                {
                    return Lock(player, player.Tribesman.Tribe);
                }

                return Lock(player);
            }
            catch(LockException)
            {
                throw;
            }
            catch(Exception)
            {
                tribe = null;
                return null;
            }
        }

        public IMultiObjectLock Lock(uint cityId, out ICity city)
        {
            return TryGetCity(cityId, out city);
        }

        public IMultiObjectLock Lock(uint strongholdId, out IStronghold stronghold)
        {
            return TryGetStronghold(strongholdId, out stronghold);
        }

        public IMultiObjectLock Lock(uint cityId, uint objectId, out ICity city, out IStructure obj)
        {
            return TryGetCityStructure(cityId, objectId, out city, out obj);
        }

        public IMultiObjectLock Lock(uint cityId, uint objectId, out ICity city, out ITroopObject obj)
        {
            return TryGetCityTroop(cityId, objectId, out city, out obj);
        }

        public IMultiObjectLock Lock(uint cityId, out ICity city, out ITribe tribe)
        {
            return TryGetCityTribe(cityId, out city, out tribe);
        }

        public IMultiObjectLock Lock(params ILockable[] list)
        {
            var lck = multiObjectLockFactory();
            lck.Lock(list);
            return lck;
        }

        public IMultiObjectLock Lock(CallbackLock.CallbackLockHandler lockHandler,
                                 object[] lockHandlerParams,
                                 params ILockable[] baseLocks)
        {
            return callbackLockFactory().Lock(lockHandler, lockHandlerParams, baseLocks);
        }

        private IMultiObjectLock TryGetCity(uint cityId, out ICity city)
        {
            if (!locator.TryGetObjects(cityId, out city))
            {
                return null;
            }

            try
            {
                return Lock(city);
            }
            catch(LockException)
            {
                throw;
            }
            catch(Exception)
            {
                city = null;
                return null;
            }
        }

        private IMultiObjectLock TryGetStronghold(uint strongholdId, out IStronghold stronghold)
        {
            if (!locator.TryGetObjects(strongholdId, out stronghold))
            {
                return null;
            }

            try
            {
                return Lock(stronghold);
            }
            catch(LockException)
            {
                throw;
            }
            catch(Exception)
            {
                stronghold = null;
                return null;
            }
        }

        private IMultiObjectLock TryGetPlayer(uint playerId, out IPlayer player)
        {
            if (!locator.TryGetObjects(playerId, out player))
            {
                return null;
            }

            try
            {
                return Lock(player);
            }
            catch(LockException)
            {
                throw;
            }
            catch(Exception)
            {
                player = null;
                return null;
            }
        }

        private IMultiObjectLock TryGetTribe(uint tribeId, out ITribe tribe)
        {
            if (!locator.TryGetObjects(tribeId, out tribe))
            {
                return null;
            }

            try
            {
                return Lock(tribe);
            }
            catch(LockException)
            {
                throw;
            }
            catch(Exception)
            {
                tribe = null;
                return null;
            }
        }

        private IMultiObjectLock TryGetCityStructure(uint cityId, uint objectId, out ICity city, out IStructure obj)
        {
            obj = null;

            var lck = TryGetCity(cityId, out city);

            if (lck == null)
            {
                return null;
            }

            city.TryGetStructure(objectId, out obj);

            return lck;
        }

        private IMultiObjectLock TryGetCityTroop(uint cityId, uint objectId, out ICity city, out ITroopObject obj)
        {
            obj = null;

            var lck = TryGetCity(cityId, out city);

            if (lck == null)
            {
                return null;
            }

            if (!city.TryGetTroop(objectId, out obj))
            {
                city = null;
                obj = null;
                lck.UnlockAll();
                return null;
            }

            return lck;
        }

        private IMultiObjectLock TryGetCityTribe(uint cityId, out ICity city, out ITribe tribe)
        {
            city = null;
            tribe = null;

            if (!locator.TryGetObjects(cityId, out city))
            {
                return null;
            }

            var lck = callbackLockFactory().Lock((custom) =>
                {
                    ICity cityParam = (ICity)custom[0];

                    return !cityParam.Owner.IsInTribe
                                   ? new ILockable[] {}
                                   : new ILockable[] {cityParam.Owner.Tribesman.Tribe};
                },
                                                 new object[] {city},
                                                 city);

            if (city.Owner.IsInTribe)
            {
                tribe = city.Owner.Tribesman.Tribe;
            }

            return lck;
        }
    }
}