using System;

namespace Game.Data.Events
{
    public class SimpleGameObjectArgs : EventArgs
    {

        public ISimpleGameObject SimpleGameObject { get; set; }

        public uint OriginalX { get; set; }

        public uint OriginalY { get; set; }

        public SimpleGameObjectArgs(ISimpleGameObject simpleGameObject)
        {
            SimpleGameObject = simpleGameObject;
        }
    }
}
