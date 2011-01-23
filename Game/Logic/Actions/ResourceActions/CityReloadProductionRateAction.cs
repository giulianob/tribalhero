using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Logic.Procedures;
using Game.Data;

namespace Game.Logic.Actions {
    public class CityReloadProductionRateAction: PassiveAction,IScriptable {
        #region IScriptable Members

        public void ScriptInit(Data.GameObject obj, string[] parms) {
            Structure structure = obj as Structure;
            if (obj==null)
                throw new Exception();
            structure.City.BeginUpdate();
            Procedure.AdjustCityResourceRates(structure, 0);
            structure.City.EndUpdate();
        }

        #endregion

        public override Setup.Error Validate(string[] parms) {
            throw new NotImplementedException();
        }

        public override Setup.Error Execute() {
            throw new NotImplementedException();
        }

        public override void UserCancelled() {
            throw new NotImplementedException();
        }

        public override void WorkerRemoved(bool wasKilled) {
            throw new NotImplementedException();
        }

        public override ActionType Type {
            get { return ActionType.CITY_RELOAD_PRODUCTION_RATE; }
        }

        public override string Properties {
            get { throw new NotImplementedException(); }
        }
    }
}
