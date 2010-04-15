#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Actions {
    public class PropertyCreateAction : PassiveAction, IScriptable {
        private Structure structure;
        private string name;
        private object value;

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            structure[name] = value;
            Global.DbManager.Save(structure);
            StateChange(ActionState.COMPLETED);

            return Error.OK;
        }

        public override void Interrupt(ActionInterrupt state) {
            throw new Exception("This action cannot be cancelled.");
        }

        public override ActionType Type {
            get { return ActionType.PROPERTY_CREATE; }
        }

        #region IScriptable Members

        public void ScriptInit(GameObject obj, string[] parms) {
            if ((structure = obj as Structure) == null)
                throw new Exception();
            name = parms[0];
            switch ((DataType) Enum.Parse(typeof (DataType), parms[1], true)) {
                case DataType.Byte:
                    value = byte.Parse(parms[2]);
                    break;
                case DataType.UShort:
                    value = ushort.Parse(parms[2]);
                    break;
                case DataType.UInt:
                    value = uint.Parse(parms[2]);
                    break;
                case DataType.String:
                    value = parms[2];
                    break;
            }
            this.Execute();
        }

        #endregion

        public override string Properties {
            get { return string.Empty; }
        }
    }
}