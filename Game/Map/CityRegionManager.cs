using Game.Data;

namespace Game.Map
{
    public class CityRegionManager : ICityRegionManager
    {
        private readonly CityRegion[] cityRegions;

        public CityRegionManager(int regionCount, ICityRegionFactory cityRegionFactory)
        {
            cityRegions = new CityRegion[regionCount];

            for (int regionId = 0; regionId < regionCount; ++regionId)
            {
                cityRegions[regionId] = cityRegionFactory.CreateCityRegion();
            }
        }

        public bool TryGetCityRegion(uint x, uint y, out CityRegion cityRegion)
        {
            return TryGetCityRegion(CityRegion.GetRegionIndex(x, y), out cityRegion);
        }

        public void Add(ICityRegionObject obj)
        {
            CityRegion cityRegion;
            if (TryGetCityRegion(obj.X, obj.Y, out cityRegion))
            {
                cityRegion.Add(obj);
            }
        }

        public void UpdateObjectRegion(ICityRegionObject obj, uint origX, uint origY)
        {
            ushort oldCityRegionId = CityRegion.GetRegionIndex(origX, origY);
            ushort newCityRegionId = CityRegion.GetRegionIndex(obj.X, obj.Y);

            if (oldCityRegionId == newCityRegionId)
            {
                cityRegions[oldCityRegionId].Update(obj, origX, origY);
            }
            else
            {
                cityRegions[oldCityRegionId].Remove(obj);
                cityRegions[newCityRegionId].Add(obj);
            }
        }

        public void Remove(ICityRegionObject obj)
        {
            CityRegion cityRegion;
            if (TryGetCityRegion(obj.X, obj.Y, out cityRegion))
            {
                cityRegion.Remove(obj);
            }
        }

        public bool TryGetCityRegion(ushort id, out CityRegion cityRegion)
        {
            cityRegion = null;
            if (id >= cityRegions.Length)
            {
                return false;
            }

            cityRegion = cityRegions[id];
            return true;
        }
    }
}