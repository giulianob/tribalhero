using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Logic.Actions {
    class AlertAction : Action, ISchedule, IScriptable {
        int interval;
        byte radius;
        DateTime time;
        GameObject obj;

        public AlertAction() {
        }

        public AlertAction(GameObject obj, int interval, byte radius) {
            this.obj = obj;
            this.interval = interval;
            this.radius = radius;
        }

        #region ICanInit Members

        public void ScriptInit(Game.Data.GameObject obj, string[] parms) {
            this.interval = int.Parse(parms[0]);
            this.radius = byte.Parse(parms[1]);
            this.obj = obj;
            this.execute();
        }

        #endregion

        #region ISchedule Members

        public DateTime Time {
            get { return time; }
        }

        public object Custom {
            get { return null; }
        }

        public void callback(object custom) {
            string property = "";
            foreach (GameObject each in Global.World.getObjectsWithin(obj.X, obj.Y, this.radius)) {
                property += each.Type + "\r\n";
            }
            //          obj["in_range"] = property;
            this.time = DateTime.Now.AddSeconds(interval);
            Scheduler.put(this);
        }

        #endregion

        #region IAction Members

        public override ActionType Type {
            get { return ActionType.ALERT; }
        }



        public override int execute() {
            this.time = DateTime.Now.AddSeconds(interval);
            Scheduler.put(this);
            return 0;
        }

        public override bool validate(string[] parms) {
            return true;
        }

        #endregion

        public override void interrupt(ActionInterrupt state) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
