#region

using System;
using System.Data;
using Game.Database;
using Game.Setup;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public abstract class ChainAction : PassiveAction, IActionTime
    {
        #region Delegates

        public delegate void ChainCallback(ActionState state);

        #endregion

        public new const string DB_TABLE = "chain_actions";

        private ChainCallback chainCallback;

        private ActionState chainState = ActionState.Started;

        protected ChainAction()
        {
        }

        protected ChainAction(uint id,
                              string chainCallback,
                              PassiveAction current,
                              ActionState chainState,
                              bool isVisible)
        {
            ActionId = id;
            this.chainState = chainState;
            Current = current;
            IsVisible = isVisible;

            //set the chain callback through reflection. The chain callback method should always exist in this class
            if (chainCallback != string.Empty)
            {
                this.chainCallback =
                        (ChainCallback)Delegate.CreateDelegate(typeof(ChainCallback), this, chainCallback, true);
            }

            switch(chainState)
            {
                case ActionState.Completed:
                case ActionState.Failed:
                    Scheduler.Current.Put(new ChainExecuter(this.chainCallback, chainState));
                    break;
                default:
                    current.IsChain = true;
                    current.OnNotify += ChainNotify;

                    ScheduledPassiveAction scheduledPassiveAction = current as ScheduledPassiveAction;
                    if (scheduledPassiveAction != null)
                    {
                        Scheduler.Current.Put(scheduledPassiveAction);
                    }

                    break;
            }
        }

        private PassiveAction Current { get; set; }

        public ActionState ChainState
        {
            get
            {
                return chainState;
            }
        }

        public override string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public override DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("type", Type, DbType.UInt32), new DbColumn("is_visible", IsVisible, DbType.Boolean),
                        new DbColumn("current_action_id",
                                     Current != null ? (object)Current.ActionId : null,
                                     DbType.UInt32),
                        new DbColumn("chain_callback",
                                     chainCallback != null ? chainCallback.Method.Name : null,
                                     DbType.String),
                        new DbColumn("chain_state", (byte)chainState, DbType.Byte),
                        new DbColumn("properties", Properties, DbType.String)
                };
            }
        }

        #region IActionTime Members

        public DateTime BeginTime
        {
            get
            {
                if (Current == null || !(Current is IActionTime))
                {
                    return DateTime.MinValue;
                }

                return (Current as IActionTime).BeginTime;
            }
        }

        public DateTime EndTime
        {
            get
            {
                if (Current == null || !(Current is IActionTime))
                {
                    return DateTime.MinValue;
                }

                return (Current as IActionTime).EndTime;
            }
        }

        public DateTime NextTime
        {
            get
            {
                if (Current == null || !(Current is IActionTime))
                {
                    return DateTime.MinValue;
                }

                return (Current as IActionTime).NextTime;
            }
        }

        #endregion

        protected void CancelCurrentChain()
        {
            if (Current == null)
            {
                throw new Exception("No current chain action to cancel");
            }

            Current.WorkerRemoved(false);
        }

        protected void ExecuteChainAndWait(PassiveAction chainable, ChainCallback routeCallback)
        {
            if (Current != null)
            {
                throw new Exception("Previous chain action has not yet completed");
            }

            chainable.IsChain = true;
            chainable.OnNotify += ChainNotify;
            chainCallback = routeCallback;

            Current = chainable;
            Current.WorkerObject = WorkerObject;
            Current.ActionId = ActionIdGenerator.GetNext();
            Current.ActionIdGenerator = ActionIdGenerator;
            Current.Location = Location;

            DbPersistance.Current.Save(this);
            chainable.StateChange(chainable.Execute() == Error.Ok ? ActionState.Started : ActionState.Failed);

            StateChange(ActionState.Rescheduled);
        }

        private void ChainNotify(GameAction action, ActionState state)
        {
            chainState = state;
            ChainCallback currentChain = chainCallback;

            ISchedule scheduledAction = action as ISchedule;

            switch(state)
            {
                case ActionState.Fired:
                case ActionState.Started:
                case ActionState.Rescheduled:
                    DbPersistance.Current.Save(action);
                    if (scheduledAction != null)
                    {
                        Scheduler.Current.Put(scheduledAction);
                    }

                    DbPersistance.Current.Save(this);

                    Scheduler.Current.Put(new ChainExecuter(currentChain, state));
                    return;
                case ActionState.Completed:
                case ActionState.Failed:
                    ActionIdGenerator.Release(action.ActionId);
                    DbPersistance.Current.Delete(action);
                    break;
                default:
                    throw new Exception("Unexpected state " + state);
            }

            //current action is completed by either success or failure
            if (scheduledAction != null)
            {
                Scheduler.Current.Remove(scheduledAction);
            }

            action.IsDone = true;
            action.OnNotify -= ChainNotify;
            Current = null;
            DbPersistance.Current.Save(this);

            Scheduler.Current.Put(new ChainExecuter(currentChain, state));
        }

        public override void UserCancelled()
        {
            Current.UserCancelled();
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new Exception("Unsupported at the moment");
        }
    }
}