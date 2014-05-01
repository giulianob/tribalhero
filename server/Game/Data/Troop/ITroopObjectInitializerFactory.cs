using Game.Logic.Actions;

namespace Game.Data.Troop
{
    public interface ITroopObjectInitializerFactory
    {
        ITroopObjectInitializer CreateStationedTroopObjectInitializer(ITroopStub stub);

        ITroopObjectInitializer CreateStationedPartialTroopObjectInitializer(ITroopStub stub, ISimpleStub unitsToRetreat);

        ITroopObjectInitializer CreateCityTroopObjectInitializer(uint cityId, ISimpleStub simpleStub, TroopBattleGroup group, AttackMode mode);

        ITroopObjectInitializer CreateAssignmentTroopObjectInitializer(ITroopObject existingTroopObject, TroopBattleGroup group, AttackMode mode);
    }
}
