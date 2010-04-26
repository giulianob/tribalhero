#region

using System;
using System.Data;
using Game.Database;
using Game.Setup;

#endregion

namespace Game.Logic {
    public enum ActionType {
        STRUCTURE_BUILD = 101,
        STRUCTURE_UPGRADE = 102,
        STRUCTURE_CHANGE = 103,
        PROPERTY_CREATE = 104,
        LABOR_MOVE = 105,

        TROOP_MOVE = 201,
        TROOP_CREATE = 202,
        TROOP_DELETE = 203,
        STARVE = 210,
        ATTACK = 250,
        DEFENSE = 251,
        RETREAT = 252,

        RESOURCE_CONVERT = 301,
        FARM = 302,
        RESOURCE = 303,
        REFINERY = 304,
        RESOURCE_MILL = 305,
        RESOURCE_BUY = 306,
        RESOURCE_SELL = 307,
        FOREST_CAMP_BUILD = 308,

        CITY_LABOR = 501,
        CITY = 502,

        TECHNOLOGY_UPGRADE = 402,
        UNIT_TRAIN = 601,
        UNIT_UPGRADE = 602,
        BATTLE = 701,
        ENGAGE_ATTACK = 702,
        ENGAGE_DEFENSE = 703
    }

    public enum ActionInterrupt {
        CANCEL = 0,
        ABORT = 1,
        KILLED = 2
    }

    public enum ActionState {
        COMPLETED = 0,
        STARTED = 1,
        FAILED = 2,
        FIRED = 3,
        RESCHEDULED = 4,
        INTERRUPTED = 5,
    }

    public abstract class GameAction : IPersistableObject {
        private ICanDo workerObject;

        public ICanDo WorkerObject {
            get { return workerObject; }
            set { workerObject = value; }
        }

        public delegate void ActionNotify(GameAction action, ActionState state);

        public event ActionNotify OnNotify;

        public bool isDone;

        public bool IsDone {
            get { return isDone; }
            set { isDone = value; }
        }

        private ushort actionId;

        public ushort ActionId {
            get { return actionId; }
            set { actionId = value; }
        }

        public void StateChange(ActionState state) {
            if (OnNotify != null)
                OnNotify(this, state);
        }

        public abstract Error Validate(string[] parms);
        public abstract Error Execute();
        public abstract void Interrupt(ActionInterrupt state);
        public abstract ActionType Type { get; }

        protected bool IsValid() {
            if (workerObject == null)
                return false;

            return workerObject.City != null && workerObject.City.Worker.Contains(this);
        }

        #region IPersistable Members

        public abstract string DbTable { get; }

        public virtual DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public virtual DbColumn[] DbColumns {
            get { return new DbColumn[] {}; }
        }

        public virtual DbColumn[] DbPrimaryKey {
            get {
                return new[] {
                                 new DbColumn("id", actionId, DbType.UInt16),
                                 new DbColumn("city_id", workerObject.City.Id, DbType.UInt32)
                             };
            }
        }

        public abstract String Properties { get; }

        public bool DbPersisted { get; set; }

        #endregion
    }
}