using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
namespace Game.Logic.Actions {
    class MoveAction : Action, ISchedule {
        GameObject obj;
        DateTime time;
        int sec;
        public MoveAction(GameObject obj, int sec) {
            this.sec = sec;
            this.obj = obj;

        }


        #region IAction Members

        public override int execute() {
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
            this.obj.X++;
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



        public override void interrupt(ActionInterrupt state) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
