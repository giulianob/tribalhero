using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;

namespace Game.Logic {
    public abstract class ScheduledPassiveAction : PassiveAction, ISchedule, IActionTime {

        public ScheduledPassiveAction() { }

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

        public ScheduledPassiveAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible) {
            this.ActionId = id;
            this.IsVisible = isVisible;
            this.beginTime = beginTime;
            this.nextTime = nextTime;
            this.endTime = endTime;
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("is_chain", IsChain, System.Data.DbType.Boolean),
                    new DbColumn("is_scheduled", true, System.Data.DbType.Boolean),
                    new DbColumn("is_visible", IsVisible, System.Data.DbType.Boolean),
                    new DbColumn("type", Type, System.Data.DbType.UInt32),
                    new DbColumn("begin_time", BeginTime, System.Data.DbType.DateTime),
                    new DbColumn("end_time", EndTime, System.Data.DbType.DateTime),
                    new DbColumn("next_time", nextTime, System.Data.DbType.DateTime),
                    new DbColumn("properties", Properties, System.Data.DbType.String)
                };
            }
        }
        #endregion
    }
}
