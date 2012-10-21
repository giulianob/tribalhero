using Game.Data.Troop;
using Game.Util.Locking;

namespace Game.Data
{
    public enum LocationType
    {
        City = 1,
        Stronghold = 2
    }

    public interface IStation : ILocation
    {
        ITroopManager Troops { get; }
    }

    public interface ILocation : ILockable, IXYPosition
    {
        uint LocationId { get; }
        LocationType LocationType { get; }
    }
}
