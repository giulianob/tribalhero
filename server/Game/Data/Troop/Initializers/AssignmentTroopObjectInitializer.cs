using System;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;

namespace Game.Data.Troop.Initializers
{
    public class AssignmentTroopObjectInitializer : ITroopObjectInitializer
    {
        private readonly ITroopObject existingTroopObject;
        private readonly TroopBattleGroup group;
        private readonly AttackMode mode;
        private readonly Formula formula;

        private ITroopObject newTroopObject;

        public AssignmentTroopObjectInitializer(ITroopObject existingTroopObject, TroopBattleGroup group, AttackMode mode, Formula formula)
        {
            this.existingTroopObject = existingTroopObject;
            this.group = group;
            this.mode = mode;
            this.formula = formula;
        }

        public Error GetTroopObject(out ITroopObject troopObject)
        {
            if (newTroopObject != null)
            {
                troopObject = newTroopObject;
                return Error.Ok;
            }

            troopObject = existingTroopObject;
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
            throw new Exception("Should not be deleting previously created troop object");
        }
    }
}
