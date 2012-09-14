using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
    public enum StrongholdState
    {
        Inactive,

        Neutral,

        Occupied
    }

    public interface IStronghold : IHasLevel, ICityRegionObject, ILockable, ISimpleGameObject, IPersistableObject, ICanDo
    {
        uint Id { get; }

        string Name { get; }

        StrongholdState StrongholdState { get; set; }

        LazyValue Gate { get; }

        decimal VictoryPointRate { get; }

        DateTime DateOccupied { get; set; }

        ITribe Tribe { get; set; }

        ITroopManager Troops { get; }

        ITribe GateOpenTo { get; set; }

        IBattleManager Battle { get; set; }

        IEnumerable<ILockable> LockList { get; }

        IActionWorker Worker { get; }
    }
}
