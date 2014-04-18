using Game.Map;
using Game.Util.Locking;

namespace Game.Data
{
    public interface IMiniMapRegionObject : ILockable, IPrimaryPosition
    {
        MiniMapRegion.ObjectType MiniMapObjectType { get; }

        uint MiniMapGroupId { get; }

        uint MiniMapObjectId { get; }
        
        byte MiniMapSize { get; }

        byte[] GetMiniMapObjectBytes();
    }
}