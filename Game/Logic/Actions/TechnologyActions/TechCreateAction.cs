using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Actions {
    class TechnologyCreateAction : PassiveAction, IScriptable {
        Structure obj;
        uint techId;
        byte lvl;
        TimeSpan ts;

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        public override Error execute() {
            if (obj == null) return Error.OBJECT_NOT_FOUND;

            TechnologyBase tech_base = TechnologyFactory.getTechnologyBase(techId, lvl);
            if (tech_base == null) {
                return Error.OBJECT_NOT_FOUND;
            }

            Technology tech = new Technology(tech_base);
            obj.Technologies.add(tech);

            Global.dbManager.Save(obj.Technologies);

            stateChange(ActionState.COMPLETED);

            return Error.OK;
        }

        public override void interrupt(ActionInterrupt state) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override ActionType Type {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #region ICanInit Members

        public void ScriptInit(Game.Data.GameObject obj, string[] parms) {
            if ((this.obj = obj as Structure) == null) {
                throw new Exception();
            }
            this.techId = uint.Parse(parms[0]);
            this.lvl = byte.Parse(parms[1]);
            this.ts = TimeSpan.FromSeconds(int.Parse(parms[2]));
            this.execute();
        }

        #endregion

        public override string Properties {
            get { return string.Empty; }
        }
    }
}
