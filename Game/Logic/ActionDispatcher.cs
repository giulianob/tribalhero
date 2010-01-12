#region

using System;
using Game.Data;

#endregion

namespace Game.Logic {
    class ActionDispatcher : ISchedule {
        private ISchedule action;

        private ActionDispatcher(Action action) {
            this.action = action as ISchedule;
        }

        public ActionDispatcher(ScheduledActiveAction action) : this(action as Action) {}

        public ActionDispatcher(ScheduledPassiveAction action) : this(action as Action) {}

        #region ISchedule Members

        public DateTime Time {
            get { return action.Time; }
        }

        public void Callback(object custom) {
            Action action = this.action as Action;
            City city;

            try {
                city = action.WorkerObject.City;
                if (city == null)
                    throw new NullReferenceException();
            }
            catch {
                return; //structure is dead
            }

            city.Worker.FireCallback(this.action);
        }

        #endregion
    }
}