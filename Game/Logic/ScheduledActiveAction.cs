#region

using System;
using System.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    public abstract class ScheduledActiveAction : ActiveAction, IActionTime, ISchedule {
        protected ScheduledActiveAction() {}

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
            set {
                nextTime = value;
                // Cap the end time so it can never be less than the next time
                if (endTime < nextTime) endTime = nextTime;
            }
        }

        #endregion

        #region ISchedule Members

        public DateTime Time {
            get { return NextTime; }
        }

        public abstract void Callback(object custom);

        #endregion

        #region IPersistable Members

        protected ScheduledActiveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                     byte workerIndex, ushort actionCount) {
            ActionId = id;
            this.beginTime = beginTime;
            this.nextTime = nextTime;
            this.endTime = endTime;
            WorkerType = workerType;
            WorkerIndex = workerIndex;
            ActionCount = actionCount;
        }

        public override DbColumn[] DbColumns {
            get {
                return new[] {
                                new DbColumn("object_id", WorkerObject.WorkerId, DbType.UInt32),
                                new DbColumn("type", Type, DbType.UInt32),
                                new DbColumn("begin_time", BeginTime, DbType.DateTime),
                                new DbColumn("end_time", EndTime, DbType.DateTime),
                                new DbColumn("next_time", nextTime, DbType.DateTime),
                                new DbColumn("worker_type", WorkerType, DbType.Int32),
                                new DbColumn("worker_index", WorkerIndex, DbType.Byte),
                                new DbColumn("count", ActionCount, DbType.UInt16),
                                new DbColumn("properties", Properties, DbType.String)
                };
            }
        }

        #endregion
    }
}