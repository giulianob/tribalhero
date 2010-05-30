#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Actions {
    class TechnologyCreateAction : PassiveAction, IScriptable {
        private Structure obj;
        private uint techId;
        private byte lvl;
        private TimeSpan ts;

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            if (obj == null)
                return Error.OBJECT_NOT_FOUND;

            TechnologyBase techBase = TechnologyFactory.GetTechnologyBase(techId, lvl);
            if (techBase == null)
                return Error.OBJECT_NOT_FOUND;

            Technology tech = new Technology(techBase);
            obj.Technologies.BeginUpdate();
            obj.Technologies.Add(tech);
            obj.Technologies.EndUpdate();

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
            techId = uint.Parse(parms[0]);
            lvl = byte.Parse(parms[1]);
            ts = TimeSpan.FromSeconds(int.Parse(parms[2]));
            Execute();
        }

        #endregion

        public override string Properties {
            get { return string.Empty; }
        }
    }
}