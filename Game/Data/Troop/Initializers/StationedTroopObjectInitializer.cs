using Game.Data.Stats;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;

namespace Game.Data.Troop.Initializers
{
    public class StationedTroopObjectInitializer : ITroopObjectInitializer
    {
        private readonly ITroopStub stub;

        private readonly Procedure procedure;

        private readonly Formula formula;

        private readonly IWorld world;

        private ITroopObject newTroopObject;

        private IStation originalStation;

        public StationedTroopObjectInitializer(ITroopStub stub, Procedure procedure, Formula formula, IWorld world)
        {
            this.stub = stub;
            this.procedure = procedure;
            this.formula = formula;
            this.world = world;
        }

        public Error GetTroopObject(out ITroopObject troopObject)
        {
            if (newTroopObject != null)
            {
                troopObject = newTroopObject;
                return Error.Ok;
            }

            if (stub.State != TroopState.Stationed)
            {
                troopObject = null;
                return Error.TroopNotStationed;
            }

            originalStation = stub.Station;
            uint x = stub.Station.X;
            uint y = stub.Station.Y;

            if (!stub.Station.Troops.RemoveStationed(stub.StationTroopId))
            {
                troopObject = null;
                return Error.TroopNotStationed;
            }

            troopObject = new TroopObject(stub) {X = x, Y = y + 1};
            newTroopObject = troopObject;
            
            stub.City.Add(troopObject);

            troopObject.BeginUpdate();
            troopObject.Stats = new TroopStats(formula.GetTroopRadius(stub, null), formula.GetTroopSpeed(stub));
            world.Regions.Add(troopObject);
            troopObject.EndUpdate();

            return Error.Ok;
        }

        public void DeleteTroopObject()
        {
            procedure.TroopObjectStation(newTroopObject, originalStation);
        }
    }
}
