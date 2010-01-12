#region

using System;
using System.Collections.Generic;

#endregion

namespace Game.Logic {
    class ScheduleComparer : IComparer<ISchedule> {
        #region IComparer<ISchedule> Members

        public int Compare(ISchedule x, ISchedule y) {
            return DateTime.Compare(x.Time, y.Time);
        }

        #endregion
    }

    public interface ISchedule {
        DateTime Time { get; }
        void Callback(object custom);
    }
}