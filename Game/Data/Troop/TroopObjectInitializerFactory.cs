using Game.Data.Troop.Initializers;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup.DependencyInjection;

namespace Game.Data.Troop
{
    public class TroopObjectInitializerFactory : ITroopObjectInitializerFactory
    {
        private readonly IKernel kernel;

        public TroopObjectInitializerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public StationedTroopObjectInitializer CreateStationedTroopObjectInitializer(ITroopStub stub)
        {
            return new StationedTroopObjectInitializer(stub, kernel.Get<Procedure>(), kernel.Get<Formula>(), kernel.Get<IWorld>());
        }

        public StationedPartialTroopObjectInitializer CreateStationedPartialTroopObjectInitializer(ITroopStub stub, ISimpleStub unitsToRetreat)
        {
            return new StationedPartialTroopObjectInitializer(stub, unitsToRetreat, kernel.Get<Formula>(), kernel.Get<IWorld>());
        }

        public CityTroopObjectInitializer CreateCityTroopObjectInitializer(uint cityId, ISimpleStub simpleStub, TroopBattleGroup @group, AttackMode mode)
        {
            return new CityTroopObjectInitializer(cityId, simpleStub, @group, mode, kernel.Get<IGameObjectLocator>(), kernel.Get<Formula>(), kernel.Get<Procedure>(), kernel.Get<IWorld>());
        }

        public AssignmentTroopObjectInitializer CreateAssignmentTroopObjectInitializer(ITroopObject existingTroopObject, TroopBattleGroup @group, AttackMode mode)
        {
            return new AssignmentTroopObjectInitializer(existingTroopObject, @group, mode, kernel.Get<Formula>());
        }
    }
}