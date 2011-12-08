using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;

namespace Game.Util.Locking
{
    public interface ILocker
    {
        IMultiObjectLock Lock(out Dictionary<uint, City> result, params uint[] cityIds);
        IMultiObjectLock Lock(out Dictionary<uint, Player> result, params uint[] playerIds);
        IMultiObjectLock Lock(uint tribeId, out Tribe tribe);
        IMultiObjectLock Lock(uint playerId, out Player player);
        IMultiObjectLock Lock(uint playerId, out Player player, out Tribe tribe);
        IMultiObjectLock Lock(uint cityId, out City city);
        IMultiObjectLock Lock(uint cityId, uint objectId, out City city, out Structure obj);
        IMultiObjectLock Lock(uint cityId, uint objectId, out City city, out TroopObject obj);
        IMultiObjectLock Lock(params ILockable[] list);
    }
}