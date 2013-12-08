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

            var cities = new ILockable[cityIds.Length];

            int i = 0;
            foreach (var cityId in cityIds)
            {
                ICity city;
                if (!locator.TryGetObjects(cityId, out city))
                {
                    result = null;
                    return Lock();
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
                    return Lock();
                }

                result[playerId] = player;
                players[i++] = player;
            }

            return Lock(players);
        }

        public IMultiObjectLock Lock(uint tribeId, out ITribe tribe)
        {
            return !locator.TryGetObjects(tribeId, out tribe) ? Lock() : Lock(tribe);
        }

        public IMultiObjectLock Lock(uint playerId, out IPlayer player)
        {
            return !locator.TryGetObjects(playerId, out player) ? Lock() : Lock(player);
        }

        public IMultiObjectLock Lock(uint cityId, out ICity city)
        {
            return TryGetCity(cityId, out city);
        }

        public IMultiObjectLock Lock(uint strongholdId, out IStronghold stronghold)
        {
            return !locator.TryGetObjects(strongholdId, out stronghold) ? Lock() : Lock(stronghold);
        }

        public IMultiObjectLock Lock(uint cityId, uint structureId, out ICity city, out IStructure obj)
        {
            obj = null;

            var lck = TryGetCity(cityId, out city);

            if (city == null)
            {
                return lck;
            }

            if (!city.TryGetStructure(structureId, out obj))
            {
                city = null;
                obj = null;                
                lck.UnlockAll();
                return Lock();
            }

            return lck;
        }

        public IMultiObjectLock Lock(uint cityId, uint troopObjectId, out ICity city, out ITroopObject obj)
        {
            obj = null;

            var lck = TryGetCity(cityId, out city);

            if (city == null)
            {
                return lck;
            }

            if (!city.TryGetTroop(troopObjectId, out obj))
            {
                city = null;
                obj = null;
                lck.UnlockAll();
                return Lock();
            }

            return lck;
        }

        public IMultiObjectLock Lock(uint cityId, out ICity city, out ITribe tribe)
        {
            tribe = null;

            if (!locator.TryGetObjects(cityId, out city))
            {
                return Lock();
            }

            var lck = callbackLockFactory().Lock(custom =>
            {
                ICity cityParam = (ICity)custom[0];

                return !cityParam.Owner.IsInTribe
                               ? new ILockable[] {}
                               : new ILockable[] {cityParam.Owner.Tribesman.Tribe};
            }, new object[] {city}, city);

            if (city.Owner.IsInTribe)
            {
                tribe = city.Owner.Tribesman.Tribe;
            }
            else
            {
                city = null;
                lck.UnlockAll();
                return Lock();
            }

            return lck;
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
            return !locator.TryGetObjects(cityId, out city) ? Lock() : Lock(city);
        }
    }
}