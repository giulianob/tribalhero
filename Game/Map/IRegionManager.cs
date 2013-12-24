using System;
using System.Collections.Generic;
using System.IO;
using Game.Comm;
using Game.Data;

namespace Game.Map
{
    public interface IRegionManager
    {
        event EventHandler<ObjectEvent> ObjectAdded;

        event EventHandler<ObjectEvent> ObjectRemoved;

        IMiniMapRegionManager MiniMapRegions { get; }

        bool IsValidXandY(uint x, uint y);

        IEnumerable<ISimpleGameObject> GetObjectsWithin(uint x, uint y, int radius);

        IEnumerable<ushort> GetTilesWithin(uint x, uint y, byte radius);

        bool Add(ISimpleGameObject obj);

        void DbLoaderAdd(ISimpleGameObject obj);

        void Remove(ISimpleGameObject obj);

        IEnumerable<ISimpleGameObject> GetObjectsInTile(uint x, uint y);

        IRegion GetRegion(uint x, uint y);

        IRegion GetRegion(ushort id);

        void UpdateObject(ISimpleGameObject sender, uint origX, uint origY);

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
                  uint miniMapRegionWidth,
                  uint miniMapRegionHeight);

        void Unload();

        IEnumerable<ushort> LockRegions(uint x, uint y, byte size);

        void UnlockRegions(IEnumerable<ushort> regionIds);

        void LockRegion(uint x, uint y);

        void UnlockRegion(uint x, uint y);

        void SubscribeRegion(Session session, ushort id);

        void UnsubscribeRegion(Session session, ushort id);
    }
}