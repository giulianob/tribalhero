using Game.Data.Troop.Initializers;
using Game.Logic.Actions;

namespace Game.Data.Troop
{
    public interface ITroopObjectInitializerFactory
    {
        StationedTroopObjectInitializer CreateStationedTroopObjectInitializer(ITroopStub stub);

        StationedPartialTroopObjectInitializer CreateStationedPartialTroopObjectInitializer(ITroopStub stub, ISimpleStub unitsToRetreat);

        CityTroopObjectInitializer CreateCityTroopObjectInitializer(uint cityId, ISimpleStub simpleStub, TroopBattleGroup group, AttackMode mode);

        AssignmentTroopObjectInitializer CreateAssignmentTroopObjectInitializer(ITroopObject existingTroopObject, TroopBattleGroup group, AttackMode mode);
    }
}
