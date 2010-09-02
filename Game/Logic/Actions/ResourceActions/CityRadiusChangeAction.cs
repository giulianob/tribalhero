using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Logic.Actions {
    public class CityRadiusChangeAction : PassiveAction,IScriptable {
        Structure obj;
        byte radius;

        public override Setup.Error Validate(string[] parms) {
            throw new NotImplementedException();
        }

        public override Setup.Error Execute() {
            if (obj.City.Radius < radius) {
                obj.City.BeginUpdate();
                obj.City.Radius = radius;
                obj.City.EndUpdate();
            }
            return Setup.Error.OK;
        }

        public override void UserCancelled() {
            throw new NotImplementedException();
        }

        public override void WorkerRemoved(bool wasKilled) {
            throw new NotImplementedException();
        }

        public override ActionType Type {
            get { return ActionType.CITY_RADIUS_CHANGE; }
        }

        public override string Properties {
            get { throw new NotImplementedException(); }
        }

        #region IScriptable Members

        public void ScriptInit(Data.GameObject obj, string[] parms) {
            if ((this.obj = obj as Structure) == null)
                throw new Exception();
            radius = byte.Parse(parms[0]);
            Execute();
        }

        #endregion
    }
}
