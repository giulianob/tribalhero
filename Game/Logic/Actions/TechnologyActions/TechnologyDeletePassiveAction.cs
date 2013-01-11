#region

using System;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;

#endregion

namespace Game.Logic.Actions
{
    public class TechnologyDeletePassiveAction : PassiveAction, IScriptable
    {
        private IStructure obj;

        private uint techId;

        public override ActionType Type
        {
            get
            {
                return ActionType.TechnologyDeletePassive;
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

        public void ScriptInit(IGameObject obj, string[] parms)
        {
            if ((this.obj = obj as IStructure) == null)
            {
                throw new Exception();
            }
            techId = uint.Parse(parms[0]);

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
            {
                return Error.ObjectNotFound;
            }

            obj.Technologies.BeginUpdate();
            obj.Technologies.Remove(techId);
            obj.Technologies.EndUpdate();
            Procedure.Current.OnTechnologyChange(obj);
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