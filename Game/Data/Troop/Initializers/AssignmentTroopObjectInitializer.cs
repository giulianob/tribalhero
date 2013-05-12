using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Logic.Actions;
using Game.Logic.Formulas;

namespace Game.Data.Troop.Initializers
{
    public class AssignmentTroopObjectInitializer : ITroopObjectInitializer
    {
        private readonly ITroopObject troopObject;
        private readonly TroopBattleGroup group;
        private readonly AttackMode mode;
        private readonly Formula formula;

        public AssignmentTroopObjectInitializer(ITroopObject troopObject, TroopBattleGroup group, AttackMode mode, Formula formula)
        {
            this.troopObject = troopObject;
            this.group = group;
            this.mode = mode;
            this.formula = formula;
        }

        public bool GetTroopObject(out ITroopObject troopObject)
        {
            troopObject = this.troopObject;
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
        }
    }
}
