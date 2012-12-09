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
        /// <param name="outLastProbe"></param>
        /// <param name="outActionsFired"></param>
        /// <param name="schedulerSize"></param>
        /// <param name="schedulerDelta"></param>
        /// <param name="outNextFire"></param>
        void Probe(out DateTime outLastProbe,
                   out int outActionsFired,
                   out int schedulerSize,
                   out int schedulerDelta,
                   out DateTime outNextFire);

        /// <summary>
        ///     Puts the action on the scheduler
        /// </summary>
        /// <param name="schedule"></param>
        void Put(ISchedule schedule);

        /// <summary>
        ///     Removes the action from the scheduler.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        bool Remove(ISchedule schedule);
    }
}