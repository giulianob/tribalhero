using System;
using Game.Data;
using Game.Util;
using Ninject.Extensions.Logging;

namespace Game.Map
{
    public class CityRegionManager : ICityRegionManager
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly CityRegion[] cityRegions;

        public CityRegionManager(int regionCount, ICityRegionFactory cityRegionFactory)
        {
            cityRegions = new CityRegion[regionCount];

            for (int regionId = 0; regionId < regionCount; ++regionId)
            {
                cityRegions[regionId] = cityRegionFactory.CreateCityRegion();
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
            // TODO: Remove this at some point.. added this to check an existing issue
            var simpleObj = obj as ISimpleGameObject;
            if (simpleObj != null && !simpleObj.InWorld)
            {
                logger.Info("Removed update for obj that is not in world: groupId[{0}] objectId[{1}] type[{2}] currentX[{3}] currentY[{4}] origX[{5}] origY[{6}]",
                         obj.CityRegionGroupId,
                         obj.CityRegionObjectId,
                         obj.CityRegionType,
                         obj.PrimaryPosition.X,
                         obj.PrimaryPosition.Y,
                         origX,
                         origY);

                throw new Exception("Received unexpected city region update for obj that is not in world");
            }

            ushort oldCityRegionId = CityRegion.GetRegionIndex(origX, origY);
            ushort newCityRegionId = CityRegion.GetRegionIndex(obj.X, obj.Y);

            if (oldCityRegionId == newCityRegionId)
            {
                logger.Info("Updated city region obj: groupId[{0}] objectId[{1}] type[{2}] regionId[{3}] currentX[{4}] currentY[{5}] origX[{6}] origY[{7}]",
                         obj.CityRegionGroupId,
                         obj.CityRegionObjectId,
                         obj.CityRegionType,
                         oldCityRegionId,
                         obj.PrimaryPosition.X,
                         obj.PrimaryPosition.Y,
                         origX,
                         origY);

                cityRegions[oldCityRegionId].Update(obj, origX, origY);
            }
            else
            {
                logger.Info("Moved city region obj: groupId[{0}] objectId[{1}] type[{2}] oldRegionId[{3}] newRegionId[{4}] currentX[{5}] currentY[{6}] origX[{7}] origY[{8}]",
                         obj.CityRegionGroupId,
                         obj.CityRegionObjectId,
                         obj.CityRegionType,
                         oldCityRegionId,
                         newCityRegionId,
                         obj.PrimaryPosition.X,
                         obj.PrimaryPosition.Y,
                         origX,
                         origY);

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