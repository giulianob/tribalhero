using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
namespace Game.Logic.Actions {
    class MoveRandomAction : Action, ISchedule, IScriptable {
        GameObject obj;
        DateTime time, starttime;
        byte sec;
        byte direction;

        public MoveRandomAction() {
        }
        public MoveRandomAction(GameObject obj, byte sec) {
            this.sec = sec;
            this.obj = obj;

        }


        #region IAction Members

        public override int execute() {
            this.starttime = DateTime.Now;
            callback(null);
            return 0;
        }

        #endregion


        #region ISchedule Members

        public DateTime Time {
            get { return time; }
        }

        public object Custom {
            get { return null; }
        }

        public void callback(object obj) {
            Console.Out.WriteLine("Time: {0}", sec);
            this.obj.BeginUpdate();
            switch (direction) {
                case 1:
                    ++this.obj.Y;
                    break;
                case 2:
                    ++this.obj.X;
                    break;
                case 3:
                    --this.obj.Y;
                    break;
                case 4:
                    --this.obj.X;
                    break;
                default:
                    return;
            }
            this.obj.EndUpdate();
            this.time = DateTime.Now.AddSeconds(sec);
            Scheduler.put(this);
        }

        #endregion

        #region IAction Members


        public int interrupt(int code) {
            throw new Exception("The method or operation is not implemented.");
        }

        public ActionState State {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region IAction Members

        public override ActionType Type {
            get { return ActionType.TROOP_MOVE; }
        }

        #endregion

        #region IAction Members


        public override bool validate(string[] parms) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICanInit Members

        public void ScriptInit(GameObject obj, string[] parms) {
            this.obj = obj;
            this.sec = byte.Parse(parms[1]);
            this.direction = byte.Parse(parms[0]);
            //        obj.Worker.do_passive(this);
        }

        #endregion



        public override void interrupt(ActionInterrupt state) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
