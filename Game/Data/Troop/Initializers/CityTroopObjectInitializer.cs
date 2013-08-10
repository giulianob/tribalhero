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

        private ITroopObject newTroopObject;

        public CityTroopObjectInitializer(uint cityId,
                                          ISimpleStub simpleStub,
                                          TroopBattleGroup group,
                                          AttackMode mode,
                                          IGameObjectLocator gameObjectLocator,
                                          Formula formula,
                                          Procedure procedure)
        {
            this.cityId = cityId;
            this.simpleStub = simpleStub;
            this.group = group;
            this.mode = mode;
            this.gameObjectLocator = gameObjectLocator;
            this.formula = formula;
            this.procedure = procedure;
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

            if (!procedure.TroopObjectCreateFromCity(city, simpleStub, city.X, city.Y, out troopObject))
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
    }
}
