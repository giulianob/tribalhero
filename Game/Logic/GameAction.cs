#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Database;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic {

    [Flags]
    public enum ActionOption {
        NOTHING  = 0,
        UNCANCELABLE = 1
    }
    
    public enum ActionType {
        OBJECT_REMOVE = 10,

        STRUCTURE_BUILD = 101,
        STRUCTURE_UPGRADE = 102,
        STRUCTURE_CHANGE = 103,
        STRUCTURE_DOWNGRADE = 104,
        PROPERTY_CREATE = 105,
        LABOR_MOVE = 106,
        STRUCTURE_USERDOWNGRADE = 107,
        STRUCTURE_SELF_DESTROY = 108,

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
        RESOURCE_SEND = 305,
        RESOURCE_BUY = 306,
        RESOURCE_SELL = 307,
        FOREST_CAMP_BUILD = 308,
        FOREST_CAMP_REMOVE = 309,
        FOREST_CAMP_HARVEST = 310,
        
        CITY_LABOR = 501,
        CITY = 502,
        CITY_RADIUS_CHANGE = 503,
        ROAD_BUILD = 510,
        ROAD_DESTROY = 511,

        TECH_CREATE = 400,
        TECHNOLOGY_UPGRADE = 402,        

        UNIT_TRAIN = 601,
        UNIT_UPGRADE = 602,
        BATTLE = 701,
        ENGAGE_ATTACK = 702,
        ENGAGE_DEFENSE = 703,
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

        private uint actionId;

        public uint ActionId {
            get { return actionId; }
            set { actionId = value; }
        }

        public void StateChange(ActionState state) {
            if (OnNotify != null)
                OnNotify(this, state);
        }
        
        public abstract Error Validate(string[] parms);
        public abstract Error Execute();
        public abstract void UserCancelled();
        public abstract void WorkerRemoved(bool wasKilled);
        public abstract ActionType Type { get; }

        protected bool IsValid() {
            return !isDone;
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