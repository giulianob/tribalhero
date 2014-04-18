using System;
using Game.Data;

namespace Game.Map
{
    public class ObjectEvent : EventArgs
    {
        public ISimpleGameObject GameObject { get; private set; }

        public ObjectEvent(ISimpleGameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }
}