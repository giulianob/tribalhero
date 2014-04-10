using System;

namespace Game.Data.Events
{
    public class GameObjectArgs : EventArgs
    {
        public IGameObject Object { get; set; }

        public uint OriginalX { get; set; }

        public uint OriginalY { get; set; }
    }
}
