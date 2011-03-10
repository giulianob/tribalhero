using Game.Map;

namespace Game.Data
{
    public interface ICityRegionObject
    {
        Map.Location GetCityRegionLocation();
        byte[] GetCityRegionObjectBytes();
        CityRegion.ObjectType GetCityRegionType();
    }
}
