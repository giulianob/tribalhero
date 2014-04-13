using System;
using Game.Data.Troop;

namespace Game.Data.Events
{
    public class TroopStubEventArgs : EventArgs
    {
        public ITroopStub Stub { get; set; }
    }
}
