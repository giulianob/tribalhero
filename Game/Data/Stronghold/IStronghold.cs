using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
    public enum  StrongholdState
    {
        Inactive,
        Neutral,
        Occupied
    }
    public interface IStronghold : IHasLevel, ICityRegionObject, ILockable
    {
        uint Id { get; }
        string Name { get; }
        ushort Radius { get; }
        StrongholdState StrongholdState { set; get; }
        LazyValue Gate { get; }
        ITribe Tribe { set;  get; }
        ITroopManager TroopManager { get; }
    }
}
