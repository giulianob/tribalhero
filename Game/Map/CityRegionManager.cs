using Game.Data;

namespace Game.Map
{
    public class CityRegionManager : ICityRegionManager
    {
        private readonly CityRegion[] cityRegions;

        public CityRegionManager(int regionCount)
        {
            cityRegions = new CityRegion[regionCount];

            for (int regionId = 0; regionId < regionCount; ++regionId)
            {
                cityRegions[regionId] = new CityRegion();
            }
        }

        public CityRegion GetCityRegion(uint x, uint y)
        {
            if (x == 0 && y == 0)
            {
                return null;
            }

            return GetCityRegion(CityRegion.GetRegionIndex(x, y));
        }

        public void Add(ICityRegionObject obj)
        {
            CityRegion cityRegion = GetCityRegion(obj.X, obj.Y);
            if (cityRegion != null)
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
            CityRegion cityRegion = GetCityRegion(obj.X, obj.Y);
            if (cityRegion != null)
            {
                cityRegion.Remove(obj);
            }
        }

        public CityRegion GetCityRegion(ushort id)
        {
            return cityRegions[id];
        }
    }
}