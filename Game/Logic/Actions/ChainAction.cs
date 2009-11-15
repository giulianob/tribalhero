using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;
using System.Reflection;
using Game.Util;

namespace Game.Logic.Actions {
    public abstract class ChainAction : PassiveAction, IPersistableObject, IActionTime {
        PassiveAction current;
        protected PassiveAction Current {
            get { return current; }
        }

        ActionState chainState = ActionState.STARTED;
        public ActionState ChainState {
            get { return chainState; }
        }

        ChainCallback chainCallback;

        public delegate void ChainCallback(ActionState state);

        public ChainAction() { }

        protected void ExecuteChainAndWait(PassiveAction chainable, ChainCallback routeCallback) {
            chainable.IsChain = true;
            chainable.OnNotify += new ActionNotify(ChainNotify);
            chainCallback = routeCallback;

            current = chainable;
            current.WorkerObject = WorkerObject;
            current.ActionId = (ushort)WorkerObject.City.Worker.getId();

            Global.dbManager.Save(this);
            if (chainable.execute() == Game.Setup.Error.OK) {
                chainable.stateChange(ActionState.STARTED);
            }
            else {
                chainable.stateChange(ActionState.FAILED);
            }

            stateChange(ActionState.RESCHEDULED);
        }

        void ChainNotify(Action action, ActionState state) {

            chainState = state;

            switch (state) {
                case ActionState.FIRED:
                case ActionState.STARTED:
                case ActionState.RESCHEDULED:
                    Global.dbManager.Save(action);
                    if (action is ScheduledPassiveAction)
                        Scheduler.put(new ActionDispatcher(action as ScheduledPassiveAction));                    
                    else if (action is ScheduledActiveAction)
                        Scheduler.put(new ActionDispatcher(action as ScheduledActiveAction));                    

                    Global.dbManager.Save(this);
                    
                    return;
                case ActionState.COMPLETED:
                case ActionState.INTERRUPTED:
                case ActionState.FAILED:
                    WorkerObject.City.Worker.releaseId(action.ActionId);
                    Global.dbManager.Delete(action);
                    break;
                default:
                    throw new Exception("Unexpected state " + state.ToString());                
            }

            //current action is completed by either success or failure
            action.IsDone = true;
            action.OnNotify -= ChainNotify;

            ChainCallback currentChain = chainCallback;
            Global.dbManager.Save(this);

            Scheduler.put(new ChainExecuter(currentChain, state));
        }

        public override void interrupt(ActionInterrupt state) {
            using (new MultiObjectLock(WorkerObject.City)) {
                current.interrupt(state);
            }
        }

        #region IPersistable Members
        public new const string DB_TABLE = "chain_actions";

        public override string DbTable {
            get {
                return DB_TABLE;
            }
        }

        public ChainAction(ushort id, string chainCallback, PassiveAction current, ActionState chainState, bool isVisible) {
            this.ActionId = id;
            this.chainState = chainState;
            this.current = current;
            this.IsVisible = isVisible;

            //set the chain callback through reflection. The chain callback method should always exist in this class
            if (chainCallback != string.Empty)
                this.chainCallback = (ChainCallback)Delegate.CreateDelegate(typeof(ChainCallback), this, chainCallback, true);

            switch (chainState) {
                case ActionState.COMPLETED:
                case ActionState.FAILED:
                case ActionState.INTERRUPTED:
                    Scheduler.put(new ChainExecuter(this.chainCallback, chainState));                    
                    break;
                default:
                    current.IsChain = true;
                    current.OnNotify += new ActionNotify(ChainNotify);

                    if (current is ScheduledPassiveAction) {
                        Scheduler.put(new ActionDispatcher(current as ScheduledPassiveAction));
                    }
                    break;
            }
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("type", Type, System.Data.DbType.UInt32),
                    new DbColumn("is_visible", IsVisible, System.Data.DbType.Boolean),
                    new DbColumn("current_action_id", current != null ? (object)current.ActionId : null, System.Data.DbType.UInt16),
                    new DbColumn("chain_callback", chainCallback != null ? chainCallback.Method.Name : null, System.Data.DbType.String),
                    new DbColumn("chain_state", (byte)chainState, System.Data.DbType.Byte),
                    new DbColumn("properties", Properties, System.Data.DbType.String)
                };
            }
        }
        #endregion

        #region IActionTime Members

        public DateTime BeginTime {
            get {
                if (current == null || !(current is IActionTime))
                    return DateTime.MinValue;
                else
                    return (current as IActionTime).BeginTime;
            }
        }

        public DateTime EndTime {
            get {
                if (current == null || !(current is IActionTime))
                    return DateTime.MinValue;
                else
                    return (current as IActionTime).EndTime;
            }
        }

        public DateTime NextTime {
            get {
                if (current == null || !(current is IActionTime))
                    return DateTime.MinValue;
                else
                    return (current as IActionTime).NextTime;
            }
        }

        #endregion
    }
}
