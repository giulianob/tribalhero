using Game.Data;

namespace Game.Map
{
    public interface ICityRegionManager
    {
        CityRegion GetCityRegion(uint x, uint y);

        CityRegion GetCityRegion(ushort id);

        void Add(ICityRegionObject cityRegionObject);

        void UpdateObjectRegion(ICityRegionObject obj, uint origX, uint origY);

        void Remove(ICityRegionObject obj);
    }
}