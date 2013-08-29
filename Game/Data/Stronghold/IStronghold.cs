using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data.Tribe;
using Game.Logic;
using Game.Logic.Notifications;
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

    public interface IStronghold : IHasLevel,
                                   IMiniMapRegionObject,
                                   ISimpleGameObject,
                                   IPersistableObject,
                                   ICanDo,
                                   IStation,
                                   INotificationOwner
    {
        string Name { get; }

        new byte Lvl { get; set; }

        StrongholdState StrongholdState { get; set; }

        decimal Gate { get; set; }

        decimal VictoryPointRate { get; }

        ushort NearbyCitiesCount { get; set; }

        DateTime DateOccupied { get; set; }

        decimal BonusDays { get; set; }

        ITribe Tribe { get; set; }

        ITribe GateOpenTo { get; set; }

        IBattleManager GateBattle { get; set; }

        IBattleManager MainBattle { get; set; }

        IEnumerable<ILockable> LockList { get; }

        IActionWorker Worker { get; }

        event EventHandler<EventArgs> GateStatusChanged;

    }
}