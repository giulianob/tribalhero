﻿using System;
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

    public interface IStronghold : IHasLevel, ICityRegionObject, ISimpleGameObject, IPersistableObject, ICanDo, IStation
    {
        uint Id { get; }

        string Name { get; }

        StrongholdState StrongholdState { get; set; }

        decimal Gate { get; set; }

        decimal VictoryPointRate { get; }

        DateTime DateOccupied { get; set; }

        ITribe Tribe { get; set; }

        ITribe GateOpenTo { get; set; }

        IBattleManager GateBattle { get; set; }

        IBattleManager MainBattle { get; set; }

        IEnumerable<ILockable> LockList { get; }

        IActionWorker Worker { get; }
    }
}
