using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Logic.Actions {
    class ChainExecuter : ISchedule {

        ChainAction.ChainCallback chainCallback;
        ActionState state;

        public ChainExecuter(ChainAction.ChainCallback chainCallback, ActionState state) {
            this.chainCallback = chainCallback;
            this.state = state;
        }

        #region ISchedule Members

        public DateTime Time {
            get { return DateTime.Now; }
        }

        public void callback(object custom) {
            chainCallback(state);
        }

        #endregion
    }
}
