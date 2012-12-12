using Game.Data.Stronghold;
using Game.Data.Troop;

namespace Game.Battle.CombatGroups
{
    public interface ICombatGroupFactory
    {
        /// <summary>
        ///     Creates a combat group from the specified troop stub and formations
        /// </summary>
        /// <param name="battleId">Battle id of the owner</param>
        /// <param name="id">Group id</param>
        /// <param name="troopStub">Troop stub to create group from</param>
        /// <returns></returns>
        CityDefensiveCombatGroup CreateCityDefensiveCombatGroup(uint battleId, uint id, ITroopStub troopStub);

        /// <summary>
        ///     Creates a combat group from the given troop object
        /// </summary>
        /// <param name="battleId">Battle id of the owner</param>
        /// <param name="id">Group id</param>
        /// <param name="troopObject">Troop object to create group from</param>
        /// <returns></returns>
        CityOffensiveCombatGroup CreateCityOffensiveCombatGroup(uint battleId, uint id, ITroopObject troopObject);

        StrongholdCombatGroup CreateStrongholdCombatGroup(uint battleId, uint id, IStronghold stronghold);
    }
}