using Game.Map;
using Game.Util.Locking;

namespace Game.Data
{
    public interface ICityRegionObject : ILockable, IPrimaryPosition
    {
        ushort CityRegionRelX { get; }

        ushort CityRegionRelY { get; }

        CityRegion.ObjectType CityRegionType { get; }

        uint CityRegionGroupId { get; }

        uint CityRegionObjectId { get; }

        byte[] GetCityRegionObjectBytes();
    }
}