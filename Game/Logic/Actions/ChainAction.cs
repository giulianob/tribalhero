#region

using System;
using System.Data;
using Game.Data;
using Game.Database;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    public abstract class ChainAction : PassiveAction, IActionTime {
        private PassiveAction current;
        protected PassiveAction Current {
            get { return current; }
        }

        private ActionState chainState = ActionState.STARTED;
        public ActionState ChainState {
            get { return chainState; }
        }

        private ChainCallback chainCallback;

        public delegate void ChainCallback(ActionState state);

        protected ChainAction() {}

        protected void ExecuteChainAndWait(PassiveAction chainable, ChainCallback routeCallback) {
            chainable.IsChain = true;
            chainable.OnNotify += ChainNotify;
            chainCallback = routeCallback;

            current = chainable;
            current.WorkerObject = WorkerObject;
            current.ActionId = (ushort) WorkerObject.City.Worker.getId();

            Global.dbManager.Save(this);
            if (chainable.Execute() == Error.OK)
                chainable.StateChange(ActionState.STARTED);
            else
                chainable.StateChange(ActionState.FAILED);

            StateChange(ActionState.RESCHEDULED);
        }

        private void ChainNotify(Action action, ActionState state) {
            chainState = state;

            switch (state) {
                case ActionState.FIRED:
                case ActionState.STARTED:
                case ActionState.RESCHEDULED:
                    Global.dbManager.Save(action);
                    if (action is ScheduledPassiveAction)
                        Global.Scheduler.put(new ActionDispatcher(action as ScheduledPassiveAction));
                    else if (action is ScheduledActiveAction)
                        Global.Scheduler.put(new ActionDispatcher(action as ScheduledActiveAction));

                    Global.dbManager.Save(this);

                    return;
                case ActionState.COMPLETED:
                case ActionState.INTERRUPTED:
                case ActionState.FAILED:
                    WorkerObject.City.Worker.releaseId(action.ActionId);
                    Global.dbManager.Delete(action);
                    break;
                default:
                    throw new Exception("Unexpected state " + state);
            }

            //current action is completed by either success or failure
            action.IsDone = true;
            action.OnNotify -= ChainNotify;

            ChainCallback currentChain = chainCallback;
            Global.dbManager.Save(this);

            Global.Scheduler.put(new ChainExecuter(currentChain, state));
        }

        public override void Interrupt(ActionInterrupt state) {
            using (new MultiObjectLock(WorkerObject.City))
                current.Interrupt(state);
        }

        #region IPersistable Members

        public new const string DB_TABLE = "chain_actions";

        public override string DbTable {
            get { return DB_TABLE; }
        }

        protected ChainAction(ushort id, string chainCallback, PassiveAction current, ActionState chainState,
                           bool isVisible) {
            ActionId = id;
            this.chainState = chainState;
            this.current = current;
            IsVisible = isVisible;

            //set the chain callback through reflection. The chain callback method should always exist in this class
            if (chainCallback != string.Empty)
                this.chainCallback =
                    (ChainCallback) Delegate.CreateDelegate(typeof (ChainCallback), this, chainCallback, true);

            switch (chainState) {
                case ActionState.COMPLETED:
                case ActionState.FAILED:
                case ActionState.INTERRUPTED:
                    Global.Scheduler.put(new ChainExecuter(this.chainCallback, chainState));
                    break;
                default:
                    current.IsChain = true;
                    current.OnNotify += ChainNotify;

                    if (current is ScheduledPassiveAction)
                        Global.Scheduler.put(new ActionDispatcher(current as ScheduledPassiveAction));
                    break;
            }
        }

        public override DbColumn[] DbColumns {
            get {
                return new[] {
                                          new DbColumn("type", Type, DbType.UInt32), new DbColumn("is_visible", IsVisible, DbType.Boolean)
                                          ,
                                          new DbColumn("current_action_id", current != null ? (object) current.ActionId : null,
                                                       DbType.UInt16),
                                          new DbColumn("chain_callback", chainCallback != null ? chainCallback.Method.Name : null,
                                                       DbType.String), new DbColumn("chain_state", (byte) chainState, DbType.Byte),
                                          new DbColumn("properties", Properties, DbType.String)
                                      };
            }
        }

        #endregion

        #region IActionTime Members

        public DateTime BeginTime {
            get {
                if (current == null || !(current is IActionTime))
                    return DateTime.MinValue;
                
                return (current as IActionTime).BeginTime;
            }
        }

        public DateTime EndTime {
            get {
                if (current == null || !(current is IActionTime))
                    return DateTime.MinValue;
                                
                return (current as IActionTime).EndTime;
            }
        }

        public DateTime NextTime {
            get {
                if (current == null || !(current is IActionTime))
                    return DateTime.MinValue;
                
                return (current as IActionTime).NextTime;
            }
        }

        #endregion
    }
}