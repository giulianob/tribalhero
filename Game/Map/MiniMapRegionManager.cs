using System;
using Game.Data;
using Game.Util;
using Ninject.Extensions.Logging;

namespace Game.Map
{
    public class MiniMapRegionManager : IMiniMapRegionManager
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly MiniMapRegion[] miniMapRegions;

        public MiniMapRegionManager(int regionCount, IMiniMapRegionFactory miniMapRegionFactory)
        {
            logger.Info("Creating city regions");

            miniMapRegions = new MiniMapRegion[regionCount];

            for (int regionId = 0; regionId < regionCount; ++regionId)
            {
                miniMapRegions[regionId] = miniMapRegionFactory.CreateMiniMapRegion();
            }
        }

        public bool TryGetMiniMapRegion(uint x, uint y, out MiniMapRegion miniMapRegion)
        {
            return TryGetMiniMapRegion(MiniMapRegion.GetRegionIndex(x, y), out miniMapRegion);
        }

        public void Add(IMiniMapRegionObject obj)
        {
            MiniMapRegion miniMapRegion;
            if (TryGetMiniMapRegion(obj.PrimaryPosition.X, obj.PrimaryPosition.Y, out miniMapRegion))
            {
                miniMapRegion.Add(obj);
            }
        }

        public void UpdateObjectRegion(IMiniMapRegionObject obj, uint origX, uint origY)
        {
            // TODO: Remove this at some point.. added this to check an existing issue
            var simpleObj = obj as ISimpleGameObject;
            if (simpleObj != null && !simpleObj.InWorld)
            {
                if (logger.IsTraceEnabled)
                {
                    logger.Trace("Removed update for obj that is not in world: groupId[{0}] objectId[{1}] type[{2}] currentX[{3}] currentY[{4}] origX[{5}] origY[{6}]",
                                 obj.MiniMapGroupId,
                                 obj.MiniMapObjectId,
                                 obj.MiniMapObjectType,
                                 obj.PrimaryPosition.X,
                                 obj.PrimaryPosition.Y,
                                 origX,
                                 origY);
                }

                throw new Exception("Received unexpected city region update for obj that is not in world");
            }

            ushort oldMiniMapRegionId = MiniMapRegion.GetRegionIndex(origX, origY);
            ushort newMiniMapRegionId = MiniMapRegion.GetRegionIndex(obj.PrimaryPosition.X, obj.PrimaryPosition.Y);

            if (oldMiniMapRegionId == newMiniMapRegionId)
            {
                if (logger.IsTraceEnabled)
                {
                    logger.Trace("Updated city region obj: groupId[{0}] objectId[{1}] type[{2}] regionId[{3}] currentX[{4}] currentY[{5}] origX[{6}] origY[{7}]",
                                 obj.MiniMapGroupId,
                                 obj.MiniMapObjectId,
                                 obj.MiniMapObjectType,
                                 oldMiniMapRegionId,
                                 obj.PrimaryPosition.X,
                                 obj.PrimaryPosition.Y,
                                 origX,
                                 origY);
                }

                miniMapRegions[oldMiniMapRegionId].Update(obj, origX, origY);
            }
            else
            {
                if (logger.IsTraceEnabled)
                {
                    logger.Trace("Moved city region obj: groupId[{0}] objectId[{1}] type[{2}] oldRegionId[{3}] newRegionId[{4}] currentX[{5}] currentY[{6}] origX[{7}] origY[{8}]",
                                 obj.MiniMapGroupId,
                                 obj.MiniMapObjectId,
                                 obj.MiniMapObjectType,
                                 oldMiniMapRegionId,
                                 newMiniMapRegionId,
                                 obj.PrimaryPosition.X,
                                 obj.PrimaryPosition.Y,
                                 origX,
                                 origY);
                }

                miniMapRegions[oldMiniMapRegionId].Remove(obj);
                miniMapRegions[newMiniMapRegionId].Add(obj);
            }
        }

        public void Remove(IMiniMapRegionObject obj)
        {
            MiniMapRegion miniMapRegion;
            if (TryGetMiniMapRegion(obj.PrimaryPosition.X, obj.PrimaryPosition.Y, out miniMapRegion))
            {
                miniMapRegion.Remove(obj);
            }
        }

        public bool TryGetMiniMapRegion(ushort id, out MiniMapRegion miniMapRegion)
        {
            miniMapRegion = null;
            if (id >= miniMapRegions.Length)
            {
                return false;
            }

            miniMapRegion = miniMapRegions[id];
            return true;
        }
    }
}