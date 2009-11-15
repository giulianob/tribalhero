using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;

namespace Game.Logic {
    public abstract class ScheduledActiveAction : ActiveAction, IActionTime, ISchedule {

        public ScheduledActiveAction() { }

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
            set {
                nextTime = value;
            }
        }
        #endregion

        #region ISchedule Members

        public DateTime Time {
            get {
                return NextTime;
            }
        }

        public abstract void callback(object custom);

        #endregion

        #region IPersistable Members

        public ScheduledActiveAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType, byte workerIndex, ushort actionCount) {
            this.ActionId = id;
            this.beginTime = beginTime;
            this.nextTime = nextTime;
            this.endTime = endTime;
            this.workerType = workerType;
            this.workerIndex = workerIndex;
            this.actionCount = actionCount;
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("type", Type, System.Data.DbType.UInt32),
                    new DbColumn("begin_time", BeginTime, System.Data.DbType.DateTime),
                    new DbColumn("end_time", EndTime, System.Data.DbType.DateTime),
                    new DbColumn("next_time", nextTime, System.Data.DbType.DateTime),
                    new DbColumn("worker_type", workerType, System.Data.DbType.Int32),
                    new DbColumn("worker_index", workerIndex, System.Data.DbType.Byte),
                    new DbColumn("count", actionCount, System.Data.DbType.UInt16),
                    new DbColumn("properties", Properties, System.Data.DbType.String)
                };
            }
        }
        #endregion
    }
}
