using System;

namespace Game.Logic
{
    public static class Scheduler
    {
        [Obsolete("Inject IScheduler instead")]
        public static IScheduler Current { get; set; }
    }
}