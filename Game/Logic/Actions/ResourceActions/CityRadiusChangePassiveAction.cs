#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Actions
{
    public class CityRadiusChangePassiveAction : PassiveAction, IScriptable
    {
        private IStructure obj;

        private byte radius;

        public override ActionType Type
        {
            get
            {
                return ActionType.CityRadiusChangePassive;
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

            radius = byte.Parse(parms[0]);
            Execute();
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            return Error.ActionNotFound;
        }

        public override Error Execute()
        {
            if (obj.City.Radius < radius)
            {
                obj.City.BeginUpdate();
                obj.City.Radius = radius;
                obj.City.EndUpdate();
            }

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