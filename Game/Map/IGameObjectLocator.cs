using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;

namespace Game.Map
{
    public interface IGameObjectLocator
    {
        List<ISimpleGameObject> this[uint x, uint y] { get; }

        bool TryGetObjects(uint cityId, out ICity city);

        bool TryGetObjects(uint playerId, out IPlayer player);

        bool TryGetObjects(uint tribeId, out ITribe tribe);

        bool TryGetObjects(uint cityId, byte troopStubId, out ICity city, out ITroopStub troopStub);

        bool TryGetObjects(uint battleId, out IBattleManager battleManager);

        bool TryGetObjects(uint cityId, uint structureId, out ICity city, out IStructure structure);

        bool TryGetObjects(uint cityId, uint troopObjectId, out ICity city, out ITroopObject troopObject);

        bool TryGetObjects(uint cityId, out ICity city, out ITribe tribe);

        List<ISimpleGameObject> GetObjects(uint x, uint y);

        List<ISimpleGameObject> GetObjectsWithin(uint x, uint y, int radius);

        bool TryGetObjects(uint strongholdId, out IStronghold stronghold);

        bool TryGetObjects(uint barbarianTribeId, out IBarbarianTribe barbarianTribe);
    }
}