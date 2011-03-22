#region

using System;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;

#endregion

namespace Game.Logic.Actions
{
    public class CityReloadProductionRatePassiveAction : PassiveAction, IScriptable
    {
        public override ActionType Type
        {
            get
            {
                return ActionType.CityReloadProductionRatePassive;
            }
        }

        public override string Properties
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #region IScriptable Members

        public void ScriptInit(GameObject obj, string[] parms)
        {
            var structure = obj as Structure;
            if (obj == null)
                throw new Exception();
            structure.City.BeginUpdate();
            Procedure.AdjustCityResourceRates(structure, 0);
            structure.City.EndUpdate();
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            throw new NotImplementedException();
        }

        public override Error Execute()
        {
            throw new NotImplementedException();
        }

        public override void UserCancelled()
        {
            throw new NotImplementedException();
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new NotImplementedException();
        }
    }
}