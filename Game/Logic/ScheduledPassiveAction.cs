#region

using System;
using System.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    public abstract class ScheduledPassiveAction : PassiveAction, ISchedule, IActionTime {
        protected ScheduledPassiveAction() {}

        #region IActionTime Members

        protected DateTime beginTime = DateTime.MinValue;

        public DateTime BeginTime {
            get { return beginTime; }
            set { beginTime = value; }
        }

        protected DateTime endTime = DateTime.MinValue;

        public DateTime EndTime {
            get { return endTime; }
            set { endTime = value; }
        }

        protected DateTime nextTime = DateTime.MinValue;

        public DateTime NextTime {
            get {
                return nextTime == DateTime.MinValue ? endTime : nextTime;
            }
            set { nextTime = value; }
        }

        #endregion

        #region ISchedule Members

        public DateTime Time {
            get { return NextTime; }
        }

        public abstract void Callback(object custom);

        #endregion

        #region IPersistable Members

        protected ScheduledPassiveAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible) {
            ActionId = id;
            IsVisible = isVisible;
            this.beginTime = beginTime;
            this.nextTime = nextTime;
            this.endTime = endTime;
        }

        public override DbColumn[] DbColumns {
            get {
                return new[] {
                                new DbColumn("object_id", WorkerObject.WorkerId, DbType.UInt32),
                                new DbColumn("is_chain", IsChain, DbType.Boolean),
                                new DbColumn("is_scheduled", true, DbType.Boolean),
                                new DbColumn("is_visible", IsVisible, DbType.Boolean), 
                                new DbColumn("type", Type, DbType.UInt32), 
                                new DbColumn("begin_time", BeginTime, DbType.DateTime),
                                new DbColumn("end_time", EndTime, DbType.DateTime),
                                new DbColumn("next_time", nextTime, DbType.DateTime),
                                new DbColumn("properties", Properties, DbType.String)
                            };
            }
        }

        #endregion
    }
}