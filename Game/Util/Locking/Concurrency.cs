using System;

namespace Game.Util.Locking
{
    public static class Concurrency
    {
        [Obsolete("Inject ILocker instead")]
        public static ILocker Current { get; set; }
    }
}