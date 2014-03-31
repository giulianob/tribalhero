using System;
using Game.Module;

namespace Game.Logic
{
    public interface IScheduler : IGameTask
    {
        /// <summary>
        ///     Returns whether the scheduler is currently paused
        /// </summary>
        bool Paused { get; }

        /// <summary>
        ///     Probes the scheduler for some information about its status. Used for monitoring.
        /// </summary>
        void Probe(out DateTime outLastProbe,
                   out int outActionsFired,
                   out int schedulerSize,
                   out int schedulerDelta,
                   out DateTime outNextFire,
                   out int outActionTotalMilliseconds);

        /// <summary>
        ///     Puts the action on the scheduler
        /// </summary>
        void Put(ISchedule schedule);

        /// <summary>
        ///     Removes the action from the scheduler.
        /// </summary>
        bool Remove(ISchedule schedule);
        
        ISchedule ActionExecuting { get; }
    }
}