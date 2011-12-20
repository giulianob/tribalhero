using Game.Map;

namespace Game.Data
{
    public interface ICityRegionObject
    {
        ushort CityRegionRelX { get; }
        ushort CityRegionRelY { get; }
        Location CityRegionLocation { get; }
        CityRegion.ObjectType CityRegionType { get; }
        uint CityRegionGroupId { get; }
        uint CityRegionObjectId { get; }
        byte[] GetCityRegionObjectBytes();
    }
}
