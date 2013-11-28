using Game.Data.Stats;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;

namespace Game.Data.Troop.Initializers
{
    public class CityTroopObjectInitializer: ITroopObjectInitializer
    {
        private readonly uint cityId;
        private readonly ISimpleStub simpleStub;
        private readonly TroopBattleGroup group;
        private readonly AttackMode mode;
        private readonly IGameObjectLocator gameObjectLocator;
        private readonly Formula formula;
        private readonly Procedure procedure;

        private readonly IWorld world;

        private ITroopObject newTroopObject;

        public CityTroopObjectInitializer(uint cityId,
                                          ISimpleStub simpleStub,
                                          TroopBattleGroup group,
                                          AttackMode mode,
                                          IGameObjectLocator gameObjectLocator,
                                          Formula formula,
                                          Procedure procedure,
                                          IWorld world)
        {
            this.cityId = cityId;
            this.simpleStub = simpleStub;
            this.group = group;
            this.mode = mode;
            this.gameObjectLocator = gameObjectLocator;
            this.formula = formula;
            this.procedure = procedure;
            this.world = world;
        }

        public Error GetTroopObject(out ITroopObject troopObject)
        {
            if (newTroopObject != null)
            {
                troopObject = newTroopObject;
                return Error.Ok;
            }

            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                troopObject = null;
                return Error.ObjectNotFound;
            }

            if (!TroopObjectCreateFromCity(city, simpleStub, city.PrimaryPosition.X, city.PrimaryPosition.Y, out troopObject))
            {
                troopObject = null;
                return Error.TroopChanged;
            }

            newTroopObject = troopObject;

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(group);
            troopObject.Stub.InitialCount = troopObject.Stub.TotalCount;
            troopObject.Stub.RetreatCount = (ushort)formula.GetAttackModeTolerance(troopObject.Stub.TotalCount, mode);
            troopObject.Stub.AttackMode = mode;
            troopObject.Stub.EndUpdate();

            return Error.Ok;
        }

        public void DeleteTroopObject()
        {
            procedure.TroopObjectDelete(newTroopObject, true);
        }

        private bool TroopObjectCreateFromCity(ICity city,
                                                      ISimpleStub stub,
                                                      uint x,
                                                      uint y,
                                                      out ITroopObject troopObject)
        {
            if (stub.TotalCount == 0 || !city.DefaultTroop.RemoveFromFormation(FormationType.Normal, stub))
            {
                troopObject = null;
                return false;
            }

            var troopStub = city.CreateTroopStub();
            troopStub.BeginUpdate();
            troopStub.Add(stub);
            troopStub.EndUpdate();

            troopObject = city.CreateTroopObject(troopStub, x, y + 1);

            troopObject.BeginUpdate();
            troopObject.Stats = new TroopStats(formula.GetTroopRadius(troopStub, null),
                                               formula.GetTroopSpeed(troopStub));
            world.Regions.Add(troopObject);
            troopObject.EndUpdate();

            return true;
        }
    }
}
