#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Actions {
    class TechnologyDeleteAction : PassiveAction, IScriptable {
        private Structure obj;

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            if (obj == null)
                return Error.OBJECT_NOT_FOUND;

            obj.Technologies.Clear();

            StateChange(ActionState.COMPLETED);

            return Error.OK;
        }

        public override void WorkerRemoved(bool wasKilled) {            
        }

        public override void UserCancelled() {            
        }

        public override ActionType Type {
            get { return ActionType.TECH_CREATE; }
        }

        #region ICanInit Members

        public void ScriptInit(GameObject obj, string[] parms) {
            if ((this.obj = obj as Structure) == null)
                throw new Exception();
            Execute();
        }

        #endregion

        public override string Properties {
            get { return string.Empty; }
        }
    }
}