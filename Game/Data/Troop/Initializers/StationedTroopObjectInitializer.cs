using System;
using Game.Logic.Procedures;
using Game.Setup;

namespace Game.Data.Troop.Initializers
{
    public class StationedTroopObjectInitializer : ITroopObjectInitializer
    {
        private readonly ITroopStub stub;
        private readonly Procedure procedure;

        public StationedTroopObjectInitializer(ITroopStub stub, Procedure procedure)
        {
            this.stub = stub;
            this.procedure = procedure;
        }

        public bool GetTroopObject(out ITroopObject troopObject)
        {
            return procedure.TroopObjectCreateFromStation(stub, out troopObject);
        }

        public void DeleteTroopObject(ITroopObject troopObject)
        {
            procedure.TroopObjectStation(troopObject, stub.Station);
        }
    }
}
