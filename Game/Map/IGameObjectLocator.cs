using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;

namespace Game.Map
{
    public interface IGameObjectLocator
    {
        bool TryGetObjects(uint cityId, out City city);

        bool TryGetObjects(uint playerId, out Player player);

        bool TryGetObjects(uint cityId, byte troopStubId, out City city, out TroopStub troopStub);

        bool TryGetObjects(uint cityId, uint structureId, out City city, out Structure structure);

        bool TryGetObjects(uint cityId, uint troopObjectId, out City city, out TroopObject troopObject);

        List<SimpleGameObject> this[uint x, uint y] { get; }

        List<SimpleGameObject> GetObjects(uint x, uint y);

        List<SimpleGameObject> GetObjectsWithin(uint x, uint y, byte radius);
    }
}