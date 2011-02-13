#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Actions
{
    public class CityRadiusChangeAction : PassiveAction, IScriptable
    {
        private Structure obj;
        private byte radius;

        public override ActionType Type
        {
            get
            {
                return ActionType.CityRadiusChange;
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
            if ((this.obj = obj as Structure) == null)
                throw new Exception();
            radius = byte.Parse(parms[0]);
            Execute();
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new NotImplementedException();
        }
    }
}