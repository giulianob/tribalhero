using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Data.Troop;

namespace Game.Battle.CombatGroups
{
    public interface ICombatGroupFactory
    {
        CityDefensiveCombatGroup CreateCityDefensiveCombatGroup(uint battleId, uint id, ITroopStub troopStub);

        CityOffensiveCombatGroup CreateCityOffensiveCombatGroup(uint battleId, uint id, ITroopObject troopObject);

        StrongholdCombatGroup CreateStrongholdCombatGroup(uint battleId, uint id, IStronghold stronghold);

        BarbarianTribeCombatGroup CreateBarbarianTribeCombatGroup(uint battleId, uint id, IBarbarianTribe barbarianTribe);
    }
}