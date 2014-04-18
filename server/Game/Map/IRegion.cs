using System.Collections.Generic;
using Game.Data;

namespace Game.Map
{
    public interface IRegion
    {
        void AddObjectToTile(ISimpleGameObject obj, uint x, uint y);

        void RemoveObjectFromTile(ISimpleGameObject obj, uint x, uint y);

        void Add(ISimpleGameObject obj);

        void Remove(ISimpleGameObject obj);

        void Remove(ISimpleGameObject obj, uint origX, uint origY);

        void MarkAsDirty();

        IEnumerable<ISimpleGameObject> GetObjectsInTile(uint x, uint y);

        IEnumerable<ISimpleGameObject> GetPrimaryObjects();

        byte[] GetObjectBytes();

        ushort GetTileType(uint x, uint y);

        void SetTileType(uint x, uint y, ushort tileType);

        void EnterWriteLock();

        void ExitWriteLock();
    }
}