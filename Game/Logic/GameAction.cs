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
        ObjectRemovePassive = 10,

        StructureBuildActive = 101,
        StructureUpgradeActive = 102,
        StructureChangeActive = 103,
        StructureChangePassive = 5103,
        StructureDowngradePassive = 104,
        PropertyCreatePassive = 105,
        LaborMoveActive = 106,
        StructureDowngradeActive = 107,
        StructureSelfDestroyPassive = 108,
        StructureSelfDestroyActive = 109,

        TroopMovePassive = 201,
        StarvePassive = 210,
        AttackChain = 250,
        DefenseChain = 251,
        RetreatChain = 252,

        ResourceSendActive = 305,
        ResourceBuyActive = 306,
        ResourceSellActive = 307,
        ForestCampBuildActive = 308,
        ForestCampRemoveActive = 309,
        ForestCampHarvestPassive = 310,
        ResourceGatherActive = 311,

        CityPassive = 502,
        CityRadiusChangePassive = 503,
        CityReloadProductionRatePassive = 504,
        CityCreatePassive = 505,
        RoadBuildActive = 510,
        RoadDestroyActive = 511,

        TechnologyCreatePassive = 400,
        TechnologyDeletePassive = 401,
        TechnologyUpgradeActive = 402,

        UnitTrainActive = 601,
        UnitUpgradeActive = 602,
        BattlePassive = 701,
        EngageAttackPassive = 702,
        EngageDefensePassive = 703,
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

        public static double CalculateTime(int seconds, bool instantAction = true)
        {
            return CalculateTime((double)seconds, instantAction);
        }

        public static double CalculateTime(double seconds, bool instantAction = true)
        {
            if (!instantAction)
                return seconds * Config.seconds_per_unit;

            return Config.actions_instant_time ? 1 : seconds * Config.seconds_per_unit;
        }

    }
}