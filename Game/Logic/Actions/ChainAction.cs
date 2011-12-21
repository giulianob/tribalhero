#region

using System;
using System.Data;
using Game.Data;
using Game.Setup;
using Ninject;
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

        protected ChainAction(uint id, string chainCallback, PassiveAction current, ActionState chainState, bool isVisible)
        {
            ActionId = id;
            this.chainState = chainState;
            Current = current;
            IsVisible = isVisible;

            //set the chain callback through reflection. The chain callback method should always exist in this class
            if (chainCallback != string.Empty)
                this.chainCallback = (ChainCallback)Delegate.CreateDelegate(typeof(ChainCallback), this, chainCallback, true);

            switch(chainState)
            {
                case ActionState.Completed:
                case ActionState.Failed:
                    Scheduler.Current.Put(new ChainExecuter(this.chainCallback, chainState));
                    break;
                default:
                    current.IsChain = true;
                    current.OnNotify += ChainNotify;

                    if (current is ScheduledPassiveAction)
                        Scheduler.Current.Put((ScheduledPassiveAction)current);
                    break;
            }
        }

        protected PassiveAction Current { get; private set; }

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
                               new DbColumn("current_action_id", Current != null ? (object)Current.ActionId : null, DbType.UInt32),
                               new DbColumn("chain_callback", chainCallback != null ? chainCallback.Method.Name : null, DbType.String),
                               new DbColumn("chain_state", (byte)chainState, DbType.Byte), new DbColumn("properties", Properties, DbType.String)
                       };
            }
        }

        #region IActionTime Members

        public DateTime BeginTime
        {
            get
            {
                if (Current == null || !(Current is IActionTime))
                    return DateTime.MinValue;

                return (Current as IActionTime).BeginTime;
            }
        }

        public DateTime EndTime
        {
            get
            {
                if (Current == null || !(Current is IActionTime))
                    return DateTime.MinValue;

                return (Current as IActionTime).EndTime;
            }
        }

        public DateTime NextTime
        {
            get
            {
                if (Current == null || !(Current is IActionTime))
                    return DateTime.MinValue;

                return (Current as IActionTime).NextTime;
            }
        }

        #endregion

        protected void ExecuteChainAndWait(PassiveAction chainable, ChainCallback routeCallback)
        {
            chainable.IsChain = true;
            chainable.OnNotify += ChainNotify;
            chainCallback = routeCallback;

            Current = chainable;
            Current.WorkerObject = WorkerObject;
            Current.ActionId = (uint)WorkerObject.City.Worker.GetId();

            Ioc.Kernel.Get<IDbManager>().Save(this);
            chainable.StateChange(chainable.Execute() == Error.Ok ? ActionState.Started : ActionState.Failed);

            StateChange(ActionState.Rescheduled);
        }

        private void ChainNotify(GameAction action, ActionState state)
        {
            chainState = state;

            switch(state)
            {
                case ActionState.Fired:
                case ActionState.Started:
                case ActionState.Rescheduled:
                    Ioc.Kernel.Get<IDbManager>().Save(action);
                    if (action is ScheduledPassiveAction)
                        Scheduler.Current.Put((ScheduledPassiveAction)action);
                    else if (action is ScheduledActiveAction)
                        Scheduler.Current.Put((ScheduledActiveAction)action);

                    Ioc.Kernel.Get<IDbManager>().Save(this);

                    return;
                case ActionState.Completed:
                case ActionState.Failed:
                    WorkerObject.City.Worker.ReleaseId(action.ActionId);
                    Ioc.Kernel.Get<IDbManager>().Delete(action);
                    break;
                default:
                    throw new Exception("Unexpected state " + state);
            }

            //current action is completed by either success or failure
            action.IsDone = true;
            action.OnNotify -= ChainNotify;

            ChainCallback currentChain = chainCallback;
            Ioc.Kernel.Get<IDbManager>().Save(this);

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