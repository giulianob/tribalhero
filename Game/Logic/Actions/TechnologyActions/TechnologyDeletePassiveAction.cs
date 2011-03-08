#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Actions
{
    class TechnologyDeletePassiveAction : PassiveAction, IScriptable
    {
        private Structure obj;

        public override ActionType Type
        {
            get
            {
                return ActionType.TechnologyCreatePassive;
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

        public void ScriptInit(GameObject obj, string[] parms)
        {
            if ((this.obj = obj as Structure) == null)
                throw new Exception();
            Execute();
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            if (obj == null)
                return Error.ObjectNotFound;

            obj.Technologies.BeginUpdate();
            obj.Technologies.Clear();
            obj.Technologies.EndUpdate();

            StateChange(ActionState.Completed);

            return Error.Ok;
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }

        public override void UserCancelled()
        {
        }
    }
}