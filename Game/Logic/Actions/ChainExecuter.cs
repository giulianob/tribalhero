#region

using System;

#endregion

namespace Game.Logic.Actions {
    class ChainExecuter : ISchedule {
        private ChainAction.ChainCallback chainCallback;
        private ActionState state;

        public ChainExecuter(ChainAction.ChainCallback chainCallback, ActionState state) {
            this.chainCallback = chainCallback;
            this.state = state;
        }

        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time {
            get { return DateTime.UtcNow; }
        }

        public void Callback(object custom) {
            chainCallback(state);
        }

        #endregion
    }
}