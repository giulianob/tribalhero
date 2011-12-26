using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;

namespace Game.Map
{
    public interface IGameObjectLocator
    {
        bool TryGetObjects(uint cityId, out ICity city);

        bool TryGetObjects(uint playerId, out IPlayer player);

        bool TryGetObjects(uint cityId, byte troopStubId, out ICity city, out ITroopStub troopStub);

        bool TryGetObjects(uint cityId, uint structureId, out ICity city, out IStructure structure);

        bool TryGetObjects(uint cityId, uint troopObjectId, out ICity city, out ITroopObject troopObject);

        List<ISimpleGameObject> this[uint x, uint y] { get; }

        List<ISimpleGameObject> GetObjects(uint x, uint y);

        List<ISimpleGameObject> GetObjectsWithin(uint x, uint y, byte radius);
    }
}