using System;

namespace Game.Data
{
    public class PlayerChatState
    {
        public PlayerChatState()
        {
        }

        public DateTime ChatFloodTime { get; set; }

        public DateTime ChatLastMessage { get; set; }

        public int ChatFloodCount { get; set; }

        public bool Distinguish { get; set; }
    }
}