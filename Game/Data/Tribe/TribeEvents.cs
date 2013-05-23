using System;

namespace Game.Data.Tribe
{
    public class TribesmanRemovedEventArgs : EventArgs
    {
        public IPlayer Player { get; set; }
    }
}