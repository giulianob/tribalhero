#region

using System;
using Game.Data;

#endregion

namespace Game.Logic {
    class ActionDispatcher : ISchedule {
        private ISchedule action;

        private ActionDispatcher(GameAction action) {
            this.action = action as ISchedule;
        }

        public ActionDispatcher(ScheduledActiveAction action) : this(action as GameAction) {}

        public ActionDispatcher(ScheduledPassiveAction action) : this(action as GameAction) {}

        #region ISchedule Members

        public DateTime Time {
            get { return action.Time; }
        }

        public void Callback(object custom) {
            GameAction action = this.action as GameAction;
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