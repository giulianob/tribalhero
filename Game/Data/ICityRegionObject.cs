using Game.Map;
using Game.Util.Locking;

namespace Game.Data
{
    public interface ICityRegionObject : IXYPosition, ILockable
    {
        ushort CityRegionRelX { get; }

        ushort CityRegionRelY { get; }

        Position CityRegionLocation { get; }

        CityRegion.ObjectType CityRegionType { get; }

        uint CityRegionGroupId { get; }

        uint CityRegionObjectId { get; }

        byte[] GetCityRegionObjectBytes();
    }
}