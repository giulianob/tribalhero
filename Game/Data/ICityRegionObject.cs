using Game.Map;

namespace Game.Data
{
    public interface ICityRegionObject : IXYPosition
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