using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Actions {
    public class PropertyCreateAction : PassiveAction, IScriptable {
        Structure structure;
        string name;
        object value;

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        public override Error execute() {
            structure[name] = value;
            Global.dbManager.Save(structure);
            stateChange(ActionState.COMPLETED);

            return Error.OK;
        }

        public override void interrupt(ActionInterrupt state) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override ActionType Type {
            get { return ActionType.PROPERTY_CREATE; }
        }

        #region IScriptable Members

        public void ScriptInit(Game.Data.GameObject obj, string[] parms) {
            if ((structure = obj as Structure) == null) {
                throw new Exception();
            }
            name = parms[0];
            switch((DataType)Enum.Parse(typeof(DataType), parms[1], true)) {
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
            this.execute();
        }

        #endregion

        public override string Properties {
            get { return string.Empty; }
        }
    }
}
