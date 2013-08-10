using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;

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

        public bool GetTroopObject(out ITroopObject troopObject)
        {
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                troopObject = null;
                return false;
            }

            if (!procedure.TroopObjectCreateFromCity(city, simpleStub, city.X, city.Y, out troopObject))
            {
                troopObject = null;
                return false;
            }

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(group);
            troopObject.Stub.InitialCount = troopObject.Stub.TotalCount;
            troopObject.Stub.RetreatCount = (ushort)formula.GetAttackModeTolerance(troopObject.Stub.TotalCount, mode);
            troopObject.Stub.AttackMode = mode;
            troopObject.Stub.EndUpdate();
            return true;
        }

        public void DeleteTroopObject(ITroopObject troopObject)
        {
            procedure.TroopObjectDelete(troopObject, true);
        }
    }
}
