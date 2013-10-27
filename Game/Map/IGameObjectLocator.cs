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
        IRegionManager Regions { get; }

        bool TryGetObjects(uint cityId, out ICity city);

        bool TryGetObjects(uint playerId, out IPlayer player);

        bool TryGetObjects(uint tribeId, out ITribe tribe);

        bool TryGetObjects(uint cityId, ushort troopStubId, out ICity city, out ITroopStub troopStub);

        bool TryGetObjects(uint battleId, out IBattleManager battleManager);

        bool TryGetObjects(uint cityId, uint structureId, out ICity city, out IStructure structure);

        bool TryGetObjects(uint cityId, uint troopObjectId, out ICity city, out ITroopObject troopObject);

        bool TryGetObjects(uint cityId, out ICity city, out ITribe tribe);

        bool TryGetObjects(uint strongholdId, out IStronghold stronghold);

        bool TryGetObjects(uint barbarianTribeId, out IBarbarianTribe barbarianTribe);
    }
}