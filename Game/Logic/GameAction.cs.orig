#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Game.Data;
using Game.Setup;
using Game.Util;
using Persistance;

#endregion

namespace Game.Logic
{
    [Flags]
    public enum ActionOption
    {
        Nothing = 0,

        Uncancelable = 1
    }

    public enum ConcurrencyType
    {
        // Standalone - Can only do this action .. this action will be disabled if any other are in progress and if you kick this off, all other actions will be blocked
        StandAlone,
        // Normal - Can only have 1 normal at a time .. Normal still allows you to do concurrent
        Normal,
        // Concurrent - Can do as many as you want but only 1 of each type
        Concurrent
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

        CityAttackChain = 250,

        CityDefenseChain = 251,

        RetreatChain = 252,

        StrongholdAttackChain = 253,

        StrongholdDefenseChain = 254,

        ResourceSendActive = 305,

        ResourceBuyActive = 306,

        ResourceSellActive = 307,

        ForestCampBuildActive = 308,

        ForestCampRemoveActive = 309,

        ForestCampHarvestPassive = 310,

        ResourceGatherActive = 311,

        TechnologyCreatePassive = 400,

        TechnologyDeletePassive = 401,

        TechnologyUpgradeActive = 402,

        CityPassive = 502,

        CityRadiusChangePassive = 503,

        CityCreatePassive = 505,

        RoadBuildActive = 510,

        RoadDestroyActive = 511,

        UnitTrainActive = 601,

        UnitUpgradeActive = 602,

        CityBattlePassive = 701,

        CityEngageAttackPassive = 702,

        CityEngageDefensePassive = 703,

        StrongholdEngageGateAttackPassive = 704,

        StrongholdGateBattlePassive = 705,

        StrongholdMainBattlePassive = 706,

        StrongholdEngageMainAttackPassive = 707,

        TribeContributeActive = 1018,
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

    public enum ActionCategory
    {
        Unspecified = 0,

        Attack = 1,

        Defense = 2
    }

    public abstract class GameAction : IPersistableObject
    {
        #region Delegates

        public delegate void ActionNotify(GameAction action, ActionState state);

        #endregion

        public ILocation Location { get; set; }

        public ICanDo WorkerObject { get; set; }

        public bool IsDone { get; set; }

        public uint ActionId { get; set; }

        public abstract ActionType Type { get; }

        public abstract String Properties { get; }

        #region IPersistableObject Members

        public abstract string DbTable { get; }

        public virtual IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[]
                {
                };
            }
        }

        public virtual DbColumn[] DbColumns
        {
            get
            {
                return new DbColumn[]
                {
                };
            }
        }

        public virtual DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[]
                {
                        new DbColumn("id", ActionId, DbType.UInt16), new DbColumn("location_type", Location.LocationType.ToString(), DbType.String), new DbColumn("location_id", Location.LocationId, DbType.UInt32)
                };
            }
        }
        
        public bool DbPersisted { get; set; }

        #endregion

        public event ActionNotify OnNotify;

        public void StateChange(ActionState state)
        {
            if (OnNotify != null)
            {
                OnNotify(this, state);
            }
        }

        public abstract Error Validate(string[] parms);

        public abstract Error Execute();

        public abstract void UserCancelled();

        public abstract void WorkerRemoved(bool wasKilled);

        protected bool IsValid()
        {
            return WorkerObject != null && !IsDone;
        }

        protected double CalculateTime(double seconds)
        {
            if (!Config.server_production && Debugger.IsAttached)
            {
                string customTime;
                string key = "actions_" + Type.ToString().ToUnderscore();
                if (Config.ExtraProperties.TryGetValue(key, out customTime))
                {
                    return double.Parse(customTime);
                }

                if (Config.actions_instant_time)
                {
                    return 1;
                }
            }

            return seconds*Config.seconds_per_unit;
        }

        /// <summary>
        /// Specified the general purpose of this action (e.g. Attack, defend, etc...)
        /// </summary>
        public virtual ActionCategory Category
        {
            get
            {
                return ActionCategory.Unspecified;
            }
        }
    }
}