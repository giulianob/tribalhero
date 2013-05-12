using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Troop.Initializers;
using Game.Logic.Actions;

namespace Game.Data.Troop
{
    public interface ITroopObjectInitializerFactory
    {
        StationedTroopObjectInitializer CreateStationedTroopObjectInitializer(ITroopStub stub);
        CityTroopObjectInitializer CreateCityTroopObjectInitializer(uint cityId, ISimpleStub simpleStub, TroopBattleGroup group, AttackMode mode);
        AssignmentTroopObjectInitializer CreateAssignmentTroopObjectInitializer(ITroopObject troopObject, TroopBattleGroup group, AttackMode mode);
    }
}
