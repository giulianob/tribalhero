using System.Collections.Generic;
using System.IO;
using Game.Comm;
using Game.Data;

namespace Game.Map
{
    public interface IRegionManager
    {
        ICityRegionManager CityRegions { get; }

        uint WorldWidth { get; }

        uint WorldHeight { get; }

        List<ISimpleGameObject> this[uint x, uint y] { get; }

        bool IsValidXandY(uint x, uint y);

        IEnumerable<ISimpleGameObject> GetObjectsFromSurroundingRegions(uint x, uint y, int radius);

        List<ISimpleGameObject> GetObjectsWithin(uint x, uint y, int radius);

        List<ushort> GetTilesWithin(uint x, uint y, byte radius);

        bool Add(ISimpleGameObject obj);

        void DbLoaderAdd(ISimpleGameObject obj);

        void Remove(ISimpleGameObject obj);

        List<ISimpleGameObject> GetObjects(uint x, uint y);

        Region GetRegion(uint x, uint y);

        Region GetRegion(ushort id);

        void ObjectUpdateEvent(ISimpleGameObject sender, uint origX, uint origY);

        ushort GetTileType(uint x, uint y);

        ushort RevertTileType(uint x, uint y, bool sendEvent);

        void SetTileType(uint x, uint y, ushort tileType, bool sendEvent);

        void Load(Stream mapStream,
                  Stream regionChangesStream,
                  bool createRegionChanges,
                  uint inWorldWidth,
                  uint inWorldHeight,
                  uint regionWidth,
                  uint regionHeight,
                  uint cityRegionWidth,
                  uint cityRegionHeight);

        void Unload();

        void LockRegion(uint x, uint y);

        void UnlockRegion(uint x, uint y);

        void SubscribeRegion(Session session, ushort id);

        void UnsubscribeRegion(Session session, ushort id);
    }
}