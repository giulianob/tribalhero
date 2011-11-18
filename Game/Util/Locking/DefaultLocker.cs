using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;

namespace Game.Util.Locking
{
    public class DefaultLocker : ILocker
    {
        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory;

        public DefaultLocker(DefaultMultiObjectLock.Factory multiObjectLockFactory)
        {
            this.multiObjectLockFactory = multiObjectLockFactory;
        }

        public IMultiObjectLock Lock(out Dictionary<uint, City> result, params uint[] cityIds)
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
                    return null;
                }

                result[cityId] = city;
                cities[i++] = city;
            }

            return Lock(cities);
        }

        public IMultiObjectLock Lock(out Dictionary<uint, Player> result, params uint[] playerIds)
        {
            result = new Dictionary<uint, Player>(playerIds.Length);
            var players = new Player[playerIds.Length];

            int i = 0;
            foreach (var playerId in playerIds) {
                Player player;
                if (!Global.World.TryGetObjects(playerId, out player)) {
                    result = null;
                    return null;
                }

                result[playerId] = player;
                players[i++] = player;
            }

            return Lock(players);
        }

        public IMultiObjectLock Lock(uint tribeId, out Tribe tribe)
        {
            return TryGetTribe(tribeId, out tribe);
        }

        public IMultiObjectLock Lock(uint playerId, out Player player)
        {
            return TryGetPlayer(playerId, out player);
        }

        public IMultiObjectLock Lock(uint playerId, out Player player, out Tribe tribe)
        {
            if (!Global.World.TryGetObjects(playerId, out player))
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
           
            try {
                tribe = player.Tribesman.Tribe;

                if (!player.Tribesman.Tribe.IsOwner(player)) {
                    return Lock(player, player.Tribesman.Tribe);
                }
                
                return Lock(player);
            } catch (LockException) {
                throw;
            } catch (Exception) {
                tribe = null;
                return null;
            }            
        }

        public IMultiObjectLock Lock(uint cityId, out City city)
        {
            return TryGetCity(cityId, out city);
        }

        public IMultiObjectLock Lock(uint cityId, uint objectId, out City city, out Structure obj)
        {
            return TryGetCityStructure(cityId, objectId, out city, out obj);
        }

        public IMultiObjectLock Lock(uint cityId, uint objectId, out City city, out TroopObject obj)
        {
            return TryGetCityTroop(cityId, objectId, out city, out obj);
        }

        public IMultiObjectLock Lock(params ILockable[] list)
        {
            var lck = multiObjectLockFactory();
            lck.Lock(list);
            return lck;
        }

        private IMultiObjectLock TryGetCity(uint cityId, out City city)
        {
            if (!Global.World.TryGetObjects(cityId, out city))
                return null;

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

        private IMultiObjectLock TryGetPlayer(uint playerId, out Player player)
        {
            if (!Global.World.TryGetObjects(playerId, out player))
                return null;

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

        private IMultiObjectLock TryGetTribe(uint tribeId, out Tribe tribe)
        {
            if (!Global.Tribes.TryGetValue(tribeId, out tribe))
                return null;

            try {
                return Lock(tribe);
            } catch (LockException) {
                throw;
            } catch (Exception) {
                tribe = null;
                return null;
            }
        }

        private IMultiObjectLock TryGetCityStructure(uint cityId, uint objectId, out City city, out Structure obj)
        {
            obj = null;

            var lck = TryGetCity(cityId, out city);

            if (lck == null)
                return null;

            if (!city.TryGetStructure(objectId, out obj))
            {
                city = null;
                obj = null;
                lck.UnlockAll();
                return null;
            }

            return lck;
        }

        private IMultiObjectLock TryGetCityTroop(uint cityId, uint objectId, out City city, out TroopObject obj)
        {
            obj = null;

            var lck = TryGetCity(cityId, out city);

            if (lck == null)
                return null;

            if (!city.TryGetTroop(objectId, out obj))
            {
                city = null;
                obj = null;
                lck.UnlockAll();
                return null;
            }

            return lck;
        }
    }
}