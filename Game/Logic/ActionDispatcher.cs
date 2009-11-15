using System;
using System.Collections.Generic;
using System.Text;
using Game.Util;
using System.Threading;

namespace Game.Logic {
    class ActionDispatcher : ISchedule {
        ISchedule action;

        ActionDispatcher(Action action) {
            this.action = action as ISchedule;
        }

        public ActionDispatcher(ScheduledActiveAction action) :
            this(action as Action) {
        }

        public ActionDispatcher(ScheduledPassiveAction action) :
            this(action as Action) {
        }

        #region ISchedule Members

        public DateTime Time {
            get { return action.Time; }
        }

        public void callback(object custom) {
            Action action = this.action as Action;
            City city;

            try {               
                city = action.WorkerObject.City;
                if (city == null) throw new NullReferenceException();
            }
            catch {
                return; //structure is dead
            }

            city.Worker.fireCallback(this.action);
        }

        #endregion
    }
}
