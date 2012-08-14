using System.Collections.Generic;
using Game.Data;

namespace Game.Map
{
    public interface IWorld
    {
        List<ISimpleGameObject> this[uint x, uint y] { get; }
        bool Add(ISimpleGameObject obj);
        void DbLoaderAdd(ISimpleGameObject obj);
        void Remove(ISimpleGameObject obj);
        List<ISimpleGameObject> GetObjects(uint x, uint y);
        List<ISimpleGameObject> GetObjectsWithin(uint x, uint y, int radius);
        void ObjectUpdateEvent(ISimpleGameObject sender, uint origX, uint origY);

        bool IsValidXandY(uint x, uint y);
        List<ushort> GetTilesWithin(uint x, uint y, byte radius);
        ushort GetTileType(uint x, uint y);
        ushort RevertTileType(uint x, uint y, bool sendEvent);
        void SetTileType(uint x, uint y, ushort tileType, bool sendEvent);
    }
}