using System.Collections.Generic;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;

namespace Game.Util.Locking
{
    public interface ILocker
    {
        IMultiObjectLock Lock(out Dictionary<uint, ICity> result, params uint[] cityIds);

        IMultiObjectLock Lock(out Dictionary<uint, IPlayer> result, params uint[] playerIds);

        IMultiObjectLock Lock(uint tribeId, out ITribe tribe);

        IMultiObjectLock Lock(uint playerId, out IPlayer player);

        IMultiObjectLock Lock(uint playerId, out IPlayer player, out ITribe tribe);

        IMultiObjectLock Lock(uint cityId, out ICity city);

        IMultiObjectLock Lock(uint strongholdId, out IStronghold stronghold);

        IMultiObjectLock Lock(uint cityId, uint objectId, out ICity city, out IStructure obj);

        IMultiObjectLock Lock(uint cityId, uint objectId, out ICity city, out ITroopObject obj);

        IMultiObjectLock Lock(uint cityId, out ICity city, out ITribe tribe);

        IMultiObjectLock Lock(params ILockable[] list);

        IMultiObjectLock Lock(CallbackLock.CallbackLockHandler lockHandler,
                          object[] lockHandlerParams,
                          params ILockable[] baseLocks);
    }
}