using System;
using Game.Logic;

namespace Game.Data.Events
{
    public class ActionWorkerEventArgs : EventArgs
    {
        public GameAction Stub { get; set; }

        public ActionState State { get; set; }
    }
}
