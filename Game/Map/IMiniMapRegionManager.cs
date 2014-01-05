using Game.Data;

namespace Game.Map
{
    public interface IMiniMapRegionManager
    {
        bool TryGetMiniMapRegion(uint x, uint y, out MiniMapRegion miniMapRegion);

        bool TryGetMiniMapRegion(ushort id, out MiniMapRegion miniMapRegion);

        void Add(IMiniMapRegionObject miniMapRegionObject);

        void UpdateObjectRegion(IMiniMapRegionObject obj, uint origX, uint origY);

        void Remove(IMiniMapRegionObject obj);
    }
}