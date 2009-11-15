using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;
using Game.Data;
using Game.Setup;

namespace Game.Logic {
    public enum ActionType : int {
        STRUCTURE_BUILD = 101,
        STRUCTURE_UPGRADE = 102,
        STRUCTURE_CHANGE = 103,
        PROPERTY_CREATE = 104,
        LABOR_MOVE = 105,

        TROOP_MOVE = 201,
        TROOP_CREATE = 202,
        TROOP_DELETE = 203,
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

        CITY_LABOR = 501,
        CITY_RESOURCE = 502,

        TECHNOLOGY_UPGRADE = 402,
        UNIT_TRAIN = 601,
        UNIT_UPGRADE = 602,
        BATTLE = 701,
        ENGAGE_ATTACK = 702,
        ENGAGE_DEFENSE = 703
    }

    public enum ActionInterrupt : int {
        CANCEL = 0,
        Abort = 1,
        KILLED = 2
    }

    public enum ActionState : int {        
        COMPLETED = 0,
        STARTED = 1,
        FAILED = 2,
        FIRED = 3,
        RESCHEDULED = 4,
        INTERRUPTED = 5,
    }

    public abstract class Action : IPersistableObject {
        ICanDo workerObject;
        public ICanDo WorkerObject {
            get { return workerObject; }
            set { workerObject = value; }
        }

        public delegate void ActionNotify(Action action, ActionState state);
        public event ActionNotify OnNotify;

        public bool isDone = false;
        public bool IsDone {
            get { return isDone; }
            set { isDone = value; }
        }

        ushort actionId;
        public ushort ActionId {
            get { return actionId; }
            set { actionId = value; }
        }

        public void stateChange(ActionState state) {
            if (OnNotify != null)
                OnNotify(this, state);            
        }

        public abstract Error validate(string[] parms);
        public abstract Error execute();
        public abstract void interrupt(ActionInterrupt state);
        public abstract ActionType Type {
            get;
        }

        protected bool isValid() {
            if (workerObject == null)
                return false;

            if (workerObject.City == null)
                return false;

            return workerObject.City.Worker.contains(this);
        }

        #region IPersistable Members

        public abstract string DbTable { get; }

        public virtual DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { };
            }
        }

        public virtual DbColumn[] DbColumns {
            get {
                return new DbColumn[] { };
            }
        }

        public virtual DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("id", actionId, System.Data.DbType.UInt16),
                    new DbColumn("city_id", workerObject.City.CityId, System.Data.DbType.UInt32),
                    new DbColumn("object_id", workerObject.WorkerId, System.Data.DbType.UInt32)
                };
            }
        }

        public abstract String Properties { get; }

        bool dbPersisted = false;
        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        #endregion
    }

}
