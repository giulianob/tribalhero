#region

using System;
using System.Collections.Generic;

#endregion

namespace Game.Logic
{
    class ScheduleComparer : IComparer<ISchedule>
    {
        #region IComparer<ISchedule> Members

        public int Compare(ISchedule x, ISchedule y)
        {
            if (x.Time.Kind != DateTimeKind.Utc)
            {
                throw new Exception(string.Format("Time is not UTC for schedule {0}", x));
            }

            if (y.Time.Kind != DateTimeKind.Utc)
            {
                throw new Exception(string.Format("Time is not UTC for schedule {0}", y));
            }

            return DateTime.Compare(x.Time, y.Time);
        }

        #endregion
    }

    public interface ISchedule
    {
        bool IsScheduled { get; set; }

        DateTime Time { get; }

        void Callback(object custom);
    }
}