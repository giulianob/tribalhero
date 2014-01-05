#region

using System;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using System.Linq;
#endregion

namespace Game.Logic.Actions
{
    public class ForestCampLaborUpdatePassiveAction : PassiveAction, IScriptable
    {
        private readonly Formula formula;
        private readonly IObjectTypeFactory objectTypeFactory;

        private IStructure obj;

        public ForestCampLaborUpdatePassiveAction(Formula formula, IObjectTypeFactory objectTypeFactory)
        {
            this.formula = formula;
            this.objectTypeFactory = objectTypeFactory;
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.ForestCampLaborUpdatePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return string.Empty;
            }
        }

        public void ScriptInit(IGameObject gameObject, string[] parms)
        {
            if ((obj = gameObject as IStructure) == null)
            {
                throw new Exception();
            }
            Execute();
        }

        public override Error Validate(string[] parms)
        {
            return Error.ActionNotFound;
        }

        public override Error Execute()
        {
            //Forest doesnt need to know the rate changed
            var lumbermill = obj.City.FirstOrDefault(s => objectTypeFactory.IsStructureType("Lumbermill", s));
            if (lumbermill == null)
            {
                return Error.LumbermillUnavailable;
            }
            
            lumbermill.BeginUpdate();
            lumbermill["Labor"] = formula.GetForestCampLaborerString(lumbermill);
            lumbermill.EndUpdate();
            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }
    }
}