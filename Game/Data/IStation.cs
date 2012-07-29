using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Troop;
using Game.Util.Locking;

namespace Game.Data
{
    public enum StationType
    {
        City = 1,
        Stronghold = 2
    }

    public interface IStation : ILocation
    {
        ITroopManager TroopManager { get; }
    }

    public interface ILocation : ILockable
    {
        uint LocationId { get; }
        StationType LocationType { get; }
        uint LocationX { get; }
        uint LocationY { get; }
    }
}
