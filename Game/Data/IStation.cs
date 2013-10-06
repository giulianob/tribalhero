using Game.Data.Troop;
using Game.Map;
using Game.Util.Locking;

namespace Game.Data
{
    public enum LocationType
    {
        City = 1,

        Stronghold = 2,

        BarbarianTribe = 3
    }

    public interface IStation : ILocation, IPrimaryPosition, ILockable
    {
        ITroopManager Troops { get; }
    }

    public interface ILocation
    {
        uint LocationId { get; }

        LocationType LocationType { get; }
    }

    public class SimpleLocation : ILocation
    {
        public SimpleLocation(LocationType locationType, uint locationId)
        {
            LocationType = locationType;
            LocationId = locationId;
        }

        public uint LocationId { get; private set; }

        public LocationType LocationType { get; private set; }
    }
}