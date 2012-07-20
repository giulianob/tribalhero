using Game.Data.Troop;

namespace Game.Battle.CombatGroups
{
    public interface ICombatGroupFactory
    {
        /// <summary>
        /// Creates a combat group from the specified troop stub and formations.
        /// </summary>
        /// <param name="battleManager">Battle manager the group will belong to. Used mainly for the battle id.</param>
        /// <param name="id">Group id</param>
        /// <param name="troopStub">Troop stub to create group from</param>
        /// <returns></returns>
        CityDefensiveCombatGroup CreateCityDefensiveCombatGroup(IBattleManager battleManager, uint id, ITroopStub troopStub);

        /// <summary>
        /// Creates a combat group from the given troop object.
        /// </summary>
        /// <param name="battleManager">Battle manager the group will belong to. Used mainly for the battle id.</param>
        /// <param name="id">Group id</param>
        /// <param name="troopObject">Troop object to create group from</param>
        /// <returns></returns>
        CityOffensiveCombatGroup CreateCityOffensiveCombatGroup(IBattleManager battleManager, uint id, ITroopObject troopObject);
    }
}
