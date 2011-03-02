#region

using System;
using System.Data;
using Game.Database;
using Game.Setup;

#endregion

namespace Game.Logic
{
    [Flags]
    public enum ActionOption
    {
        Nothing = 0,
        Uncancelable = 1
    }

    public enum ActionType
    {
        ObjectRemove = 10,

        StructureBuild = 101,
        StructureUpgrade = 102,
        StructureChange = 103,
        StructureDowngrade = 104,
        PropertyCreate = 105,
        LaborMove = 106,
        StructureUserdowngrade = 107,
        StructureSelfDestroy = 108,

        TroopMove = 201,
        TroopCreate = 202,
        TroopDelete = 203,
        Starve = 210,
        Attack = 250,
        Defense = 251,
        Retreat = 252,

        ResourceConvert = 301,
        Farm = 302,
        Resource = 303,
        Refinery = 304,
        ResourceSend = 305,
        ResourceBuy = 306,
        ResourceSell = 307,
        ForestCampBuild = 308,
        ForestCampRemove = 309,
        ForestCampHarvest = 310,
        ResourceGather = 311,

        CityLabor = 501,
        City = 502,
        CityRadiusChange = 503,
        CityReloadProductionRate = 504,
        RoadBuild = 510,
        RoadDestroy = 511,

        TechCreate = 400,
        TechnologyUpgrade = 402,

        UnitTrain = 601,
        UnitUpgrade = 602,
        Battle = 701,
        EngageAttack = 702,
        EngageDefense = 703,
    }

    public enum ActionInterrupt
    {
        Cancel = 0,
        Abort = 1,
        Killed = 2
    }

    public enum ActionState
    {
        Completed = 0,
        Started = 1,
        Failed = 2,
        Fired = 3,
        Rescheduled = 4,
    }

    public abstract class GameAction : IPersistableObject
    {
        #region Delegates

        public delegate void ActionNotify(GameAction action, ActionState state);

        #endregion

        public ICanDo WorkerObject { get; set; }

        public bool IsDone { get; set; }

        public uint ActionId { get; set; }

        public abstract ActionType Type { get; }
        public abstract String Properties { get; }

        #region IPersistableObject Members

        public abstract string DbTable { get; }

        public virtual DbDependency[] DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public virtual DbColumn[] DbColumns
        {
            get
            {
                return new DbColumn[] {};
            }
        }

        public virtual DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("id", ActionId, DbType.UInt16), new DbColumn("city_id", WorkerObject.City.Id, DbType.UInt32)};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public event ActionNotify OnNotify;

        public void StateChange(ActionState state)
        {
            if (OnNotify != null)
                OnNotify(this, state);
        }

        public abstract Error Validate(string[] parms);
        public abstract Error Execute();
        public abstract void UserCancelled();
        public abstract void WorkerRemoved(bool wasKilled);

        protected bool IsValid()
        {
            try
            {
                if (WorkerObject.City == null)
                    throw new NullReferenceException();
            }
            catch
            {
                return false; //structure is dead
            }

            return !IsDone;
        }

        protected double CalculateTime(int seconds, bool instantAction = true)
        {
            if (!instantAction)
                return seconds*Config.seconds_per_unit;

            return Config.actions_instant_time ? 1 : seconds*Config.seconds_per_unit;
        }
    }
}