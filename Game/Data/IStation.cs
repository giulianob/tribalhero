using Game.Data.Troop;
using Game.Util.Locking;

namespace Game.Data
{
    public enum LocationType
    {
        City = 1,

        Stronghold = 2,

        BarbarianTribe = 3
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

    public class SimpleLocation : ILocation
    {
        public SimpleLocation(LocationType locationType, uint locationId)
        {
            LocationType = locationType;
            LocationId = locationId;
        }

        #region Implementation of ILockable

        public int Hash { get; private set; }

        public object Lock { get; private set; }

        #endregion

        #region Implementation of IXYPosition

        public uint X { get; set; }

        public uint Y { get; set; }

        #endregion

        #region Implementation of ILocation

        public uint LocationId { get; private set; }

        public LocationType LocationType { get; private set; }

        #endregion
    }
}