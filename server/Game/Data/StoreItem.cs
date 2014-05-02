using System;

namespace Game.Data
{
    public class StoreItem
    {
        public string Id { get; set; }

        public int Type { get; set; }

        public int Cost { get; set; }

        public int MinimumVersion { get; set; }

        public DateTime Created { get; set; }
    }
}