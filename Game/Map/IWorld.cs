using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;

namespace Game.Map
{
    // TODO: IGameObjectLocator shouldnt need to be the world
    public interface IWorld : IGameObjectLocator
    {
        ICityManager Cities { get; }

        IRegionManager Regions { get; }

        RoadManager Roads { get; }

        ForestManager Forests { get; }

        IStrongholdManager Strongholds { get; }

        object Lock { get; }

        Dictionary<uint, IPlayer> Players { get; }

        int TribeCount { get; }

        int GetActivePlayerCount();

        void Add(ITribe tribe);

        void DbLoaderAdd(ITribe tribe);

        void Add(IBattleManager battleManager);

        void Remove(IBattleManager battleManager);

        void DbLoaderAdd(IBattleManager battleManager);

        void AfterDbLoaded();

        void Remove(ITribe tribe);

        bool FindPlayerId(string name, out uint playerId);        

        bool FindTribeId(string name, out uint tribeId);

        bool CityNameTaken(string name);

        bool TribeNameTaken(string name);
    }
}