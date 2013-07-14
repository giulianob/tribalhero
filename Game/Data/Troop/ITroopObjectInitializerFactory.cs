using Game.Data.Troop.Initializers;
using Game.Logic.Actions;
using Game.Util.Ninject;

namespace Game.Data.Troop
{
    public interface ITroopObjectInitializerFactory
    {
        [FactoryReturns(typeof(StationedTroopObjectInitializer))]
        ITroopObjectInitializer CreateStationedTroopObjectInitializer(ITroopStub stub);

        [FactoryReturns(typeof(CityTroopObjectInitializer))]
        ITroopObjectInitializer CreateCityTroopObjectInitializer(uint cityId, ISimpleStub simpleStub, TroopBattleGroup group, AttackMode mode);

        [FactoryReturns(typeof(AssignmentTroopObjectInitializer))]
        ITroopObjectInitializer CreateAssignmentTroopObjectInitializer(ITroopObject troopObject, TroopBattleGroup group, AttackMode mode);
    }
}
