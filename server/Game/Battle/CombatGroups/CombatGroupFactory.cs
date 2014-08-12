using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public class CombatGroupFactory : ICombatGroupFactory
    {
        private readonly IKernel kernel;

        public CombatGroupFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public CityDefensiveCombatGroup CreateCityDefensiveCombatGroup(uint battleId, uint id, ITroopStub troopStub)
        {
            return new CityDefensiveCombatGroup(battleId, id, troopStub, kernel.Get<IDbManager>());
        }

        public CityOffensiveCombatGroup CreateCityOffensiveCombatGroup(uint battleId, uint id, ITroopObject troopObject)
        {
            return new CityOffensiveCombatGroup(battleId, id, troopObject, kernel.Get<IDbManager>());
        }

        public StrongholdCombatGroup CreateStrongholdCombatGroup(uint battleId, uint id, IStronghold stronghold)
        {
            return new StrongholdCombatGroup(battleId, id, stronghold, kernel.Get<IDbManager>());
        }

        public BarbarianTribeCombatGroup CreateBarbarianTribeCombatGroup(uint battleId, uint id, IBarbarianTribe barbarianTribe)
        {
            return new BarbarianTribeCombatGroup(battleId, id, barbarianTribe, kernel.Get<IDbManager>());
        }
    }
}