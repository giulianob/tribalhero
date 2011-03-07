#region

using System;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    public class PropertyCreatePassiveAction : PassiveAction, IScriptable
    {
        private string name;
        private Structure structure;
        private object value;

        public override ActionType Type
        {
            get
            {
                return ActionType.PropertyCreatePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("value", (uint)value),});
            }
        }

        #region IScriptable Members

        public void ScriptInit(GameObject obj, string[] parms)
        {
            if ((structure = obj as Structure) == null)
                throw new Exception();
            name = parms[0];
            switch((DataType)Enum.Parse(typeof(DataType), parms[1], true))
            {
                case DataType.Byte:
                    value = byte.Parse(parms[2]);
                    break;
                case DataType.UShort:
                    value = ushort.Parse(parms[2]);
                    break;
                case DataType.Int:
                    value = int.Parse(parms[2]);
                    break;
                case DataType.UInt:
                    value = uint.Parse(parms[2]);
                    break;
                case DataType.String:
                    value = parms[2];
                    break;
            }

            Execute();
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            structure[name] = value;
            Global.DbManager.Save(structure);
            StateChange(ActionState.Completed);

            return Error.Ok;
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            using (new MultiObjectLock(structure.City))
            {
                if (!IsValid())
                    return;

                StateChange(ActionState.Failed);
            }
        }

        public override void UserCancelled()
        {
            return;
        }
    }
}