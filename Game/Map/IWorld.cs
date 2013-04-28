using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Logic.Procedures;

namespace Game.Map
{
    // TODO: IGameObjectLocator shouldnt need to be the world
    public interface IWorld : IGameObjectLocator
    {
        ICityManager Cities { get; }

        IRegionManager Regions { get; }

        RoadManager Roads { get; }

        ForestManager Forests { get; }

        object Lock { get; }

        Dictionary<uint, IPlayer> Players { get; }

        int GetActivePlayerCount();

        void Add(IBattleManager battleManager);

        void Remove(IBattleManager battleManager);

        void DbLoaderAdd(IBattleManager battleManager);

        void AfterDbLoaded(Procedure procedure);

        bool FindStrongholdId(string name, out uint strongholdId);

        bool FindPlayerId(string name, out uint playerId);

        bool CityNameTaken(string name);
    }
}