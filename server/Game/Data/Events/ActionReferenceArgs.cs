using System;
using Game.Logic;

namespace Game.Data.Events
{
    public class ActionReferenceArgs : EventArgs
    {
        public ReferenceStub ReferenceStub { get; set; }
    }
}