using Game.Data;

namespace Game.Map
{
    public interface ICityRegionManager
    {
        bool TryGetCityRegion(uint x, uint y, out CityRegion cityRegion);

        bool TryGetCityRegion(ushort id, out CityRegion cityRegion);

        void Add(ICityRegionObject cityRegionObject);

        void UpdateObjectRegion(ICityRegionObject obj, uint origX, uint origY);

        void Remove(ICityRegionObject obj);
    }
}