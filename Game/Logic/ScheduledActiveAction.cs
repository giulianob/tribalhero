#region

using System;
using System.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    public abstract class ScheduledActiveAction : ActiveAction, IActionTime, ISchedule {
        public ScheduledActiveAction() {}

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
                if (nextTime == DateTime.MinValue)
                    return endTime;
                else
                    return nextTime;
            }
            set { nextTime = value; }
        }

        #endregion

        #region ISchedule Members

        public DateTime Time {
            get { return NextTime; }
        }

        public abstract void callback(object custom);

        #endregion

        #region IPersistable Members

        public ScheduledActiveAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                     byte workerIndex, ushort actionCount) {
            ActionId = id;
            this.beginTime = beginTime;
            this.nextTime = nextTime;
            this.endTime = endTime;
            this.WorkerType = workerType;
            this.WorkerIndex = workerIndex;
            this.ActionCount = actionCount;
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
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