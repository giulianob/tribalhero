#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using System.Linq;
#endregion

namespace Game.Logic.Actions
{
    public class ForestCampRateUpdatePassiveAction : PassiveAction, IScriptable
    {
        private readonly Formula formula;
        private readonly IObjectTypeFactory objectTypeFactory;

        private IStructure obj;

        public ForestCampRateUpdatePassiveAction(Formula formula, IObjectTypeFactory objectTypeFactory)
        {
            this.formula = formula;
            this.objectTypeFactory = objectTypeFactory;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.ForestCampRateUpdatePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return string.Empty;
            }
        }

        #region IScriptable Members

        public void ScriptInit(IGameObject gameObject, string[] parms)
        {
            if ((obj = gameObject as IStructure) == null)
            {
                throw new Exception();
            }
            Execute();
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            return Error.ActionNotFound;
        }

        public override Error Execute()
        {
            //Forest doesnt need to know the rate changed
            foreach (var camp in obj.City.Where(s => objectTypeFactory.IsStructureType("ForestCamp", s)))
            {
                object efficiency;
                if (!camp.Properties.TryGet("efficiency", out efficiency))
                    continue;

                var newRate = formula.GetWoodRateForForestCamp(camp, (float)efficiency);
                var oldRate = (int)camp["Rate"];

                if (oldRate == newRate)
                    continue;

                camp.BeginUpdate();
                camp["Rate"] = newRate;
                camp.EndUpdate();

                camp.City.BeginUpdate();
                camp.City.Resource.Wood.Rate += newRate - oldRate;
                camp.City.EndUpdate();
            }

            obj.BeginUpdate();
            obj["Labor"] = formula.GetForestCampLaborerString(obj);
            obj.EndUpdate();               

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