using System.Collections.Concurrent;
using Game.Battle;
using Game.Data;
using Game.Data.Forest;
using Game.Logic.Procedures;

namespace Game.Map
{
    // TODO: IGameObjectLocator shouldnt need to be the world
    public interface IWorld : IGameObjectLocator
    {
        ICityManager Cities { get; }

        IRegionManager Regions { get; }

        IRoadManager Roads { get; }

        object Lock { get; }

        ConcurrentDictionary<uint, IPlayer> Players { get; }

        int GetActivePlayerCount();

        void Add(IBattleManager battleManager);

        void Remove(IBattleManager battleManager);

        void DbLoaderAdd(IBattleManager battleManager);

        void AfterDbLoaded(Procedure procedure, IForestManager forestManager);

        bool FindStrongholdId(string name, out uint strongholdId);

        bool FindPlayerId(string name, out uint playerId);

        bool CityNameTaken(string name);
    }
}