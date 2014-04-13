using Game.Data.Stats;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;

namespace Game.Data.Troop.Initializers
{
    /// <summary>
    /// Creates a new troop object and troop stub with the specified units from an existing stub
    /// </summary>
    public class StationedPartialTroopObjectInitializer : ITroopObjectInitializer
    {
        private readonly ITroopStub stub;

        private readonly ISimpleStub unitsToRetreat;

        private readonly Formula formula;

        private readonly IWorld world;

        private ITroopObject newTroopObject;
        
        public StationedPartialTroopObjectInitializer(ITroopStub stub,
                                                      ISimpleStub unitsToRetreat,
                                                      Formula formula,
                                                      IWorld world)
        {
            this.stub = stub;
            this.unitsToRetreat = unitsToRetreat;
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

            if (unitsToRetreat.TotalCount == 0)
            {
                troopObject = null;
                return Error.TroopEmpty;
            }

            if (stub.State != TroopState.Stationed)
            {
                troopObject = null;
                return Error.TroopNotStationed;
            }

            if (!stub.RemoveFromFormation(FormationType.Defense, unitsToRetreat))
            {
                troopObject = null;
                return Error.TroopChanged;
            }

            var newTroopStub = stub.City.CreateTroopStub();

            newTroopStub.BeginUpdate();
            newTroopStub.Add(unitsToRetreat);
            newTroopStub.EndUpdate();

            newTroopObject = stub.City.CreateTroopObject(newTroopStub, stub.Station.PrimaryPosition.X, stub.Station.PrimaryPosition.Y + 1);

            newTroopObject.BeginUpdate();
            newTroopObject.Stats = new TroopStats(formula.GetTroopRadius(stub, null),
                                                  formula.GetTroopSpeed(stub));
            world.Regions.Add(newTroopObject);
            newTroopObject.EndUpdate();

            troopObject = newTroopObject;
            return Error.Ok;
        }

        public void DeleteTroopObject()
        {
            if (!stub.City.Troops.Remove(newTroopObject.Stub.TroopId))
            {
                return;
            }

            stub.BeginUpdate();
            stub.AddAllToFormation(FormationType.Defense, newTroopObject.Stub);
            stub.EndUpdate();            
            
            newTroopObject.BeginUpdate();
            world.Regions.Remove(newTroopObject);
            stub.City.ScheduleRemove(newTroopObject, false);
            newTroopObject.EndUpdate();
        }
    }
}
