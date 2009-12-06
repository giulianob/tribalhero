using System;
using System.Collections.Generic;
using System.Text;
using Game.Setup;
using Game.Util;
using Game.Data;
using log4net;
using Game.Database;
using Game.Logic.Actions;
using System.Threading;

namespace Game.Logic {
    public class ActionRequirement {
        public byte index;
        public byte max;
        public ActionType type;
        public string[] parms;
        public uint effect_req_id;
        public EffectInheritance effect_req_inherit;
    }

    public class ActionWorker : ISchedule {
        LargeIdGenerator actionIdGen = new LargeIdGenerator(ushort.MaxValue);

        ListDictionary<ushort, ActiveAction> active = new ListDictionary<ushort, ActiveAction>();
        ListDictionary<ushort, PassiveAction> passive = new ListDictionary<ushort, PassiveAction>();

        NotificationManager notifications;
        ReferenceManager references;

        City city;

        public ActionWorker(City owner) {
            this.city = owner;
            notifications = new NotificationManager(this);
            references = new ReferenceManager(this);
        }

        #region Properties
        public byte Count {
            get { return (byte)active.Count; }
        }

        public ListDictionary<ushort, ActiveAction> ActiveActions {
            get {
                return active;
            }
        }

        public ListDictionary<ushort, PassiveAction> PassiveActions {
            get {
                return passive;
            }
        }

        public NotificationManager Notifications {
            get { return notifications; }
        }

        public ReferenceManager References {
            get { return references; }
        }

        public City City {
            get { return city; }
        }
        #endregion

        #region Private Members
        private void notifyPassive(Action action, ActionState state) {
            PassiveAction action_stub;
            if (!passive.TryGetValue(action.ActionId, out action_stub))
                return;

            switch (state) {
                case ActionState.RESCHEDULED:
                    if (ActionRescheduled != null)
                        ActionRescheduled(action_stub);

                    if (action is PassiveAction || action is ChainAction)
                        Global.dbManager.Save(action_stub);

                    if (action is ScheduledPassiveAction)
                        schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.STARTED:
                    if (ActionStarted != null)
                        ActionStarted(action_stub);

                    if (action is PassiveAction || action is ChainAction)
                        Global.dbManager.Save(action_stub);

                    if (action is ScheduledPassiveAction)
                        schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.COMPLETED:
                    passive.Remove(action_stub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(action_stub);

                    if (action is PassiveAction || action is ChainAction)
                        Global.dbManager.Delete(action_stub);
                    break;
                case ActionState.FIRED:
                    if (action is PassiveAction || action is ChainAction)
                        Global.dbManager.Save(action_stub);

                    if (action is ScheduledPassiveAction)
                        schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.INTERRUPTED:
                case ActionState.FAILED:
                    passive.Remove(action_stub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(action_stub);

                    if (action is PassiveAction || action is ChainAction)
                        Global.dbManager.Delete(action_stub);
                    break;
            }
        }

        private void notifyActive(Action action, ActionState state) {
            ActiveAction action_stub;
            if (!active.TryGetValue(action.ActionId, out action_stub))
                return;

            switch (state) {
                case ActionState.RESCHEDULED:
                    if (ActionRescheduled != null)
                        ActionRescheduled(action_stub);

                    if (action_stub is ScheduledActiveAction) {
                        Global.dbManager.Save(action_stub);
                        schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.STARTED:
                    if (ActionStarted != null)
                        ActionStarted(action_stub);

                    if (action is ScheduledActiveAction) {
                        Global.dbManager.Save(action_stub);
                        schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.COMPLETED:
                    active.Remove(action_stub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(action_stub);

                    if (action is ScheduledActiveAction)
                        Global.dbManager.Delete(action_stub);
                    break;
                case ActionState.FIRED:
                    if (action is ScheduledActiveAction) {
                        Global.dbManager.Save(action_stub);
                        schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.INTERRUPTED:
                case ActionState.FAILED:
                    active.Remove(action_stub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(action_stub);

                    if (action is ScheduledActiveAction)
                        Global.dbManager.Delete(action_stub);
                    break;
            }
        }
        #endregion

        #region DB Loader
        public void dbLoaderDoActive(ActiveAction action) {
            //this should only be used by the db loader
            if (action is ScheduledActiveAction) {
                schedule(action as ScheduledActiveAction);
            }

            action.OnNotify += this.notifyActive;
            actionIdGen.set(action.ActionId);
            active.Add(action.ActionId, action);
        }

        public void dbLoaderDoPassive(PassiveAction action) {
            //this should only be used by the db loader
            if (action is ScheduledPassiveAction) {
                schedule(action as ScheduledPassiveAction);
            }

            action.OnNotify += this.notifyPassive;
            actionIdGen.set(action.ActionId);
            passive.Add(action.ActionId, action);
        }
        #endregion

        #region Scheduling
        public Error doActive(int workerType, GameObject workerObject, ActiveAction action, IHasEffect effects) {
            int action_id = actionIdGen.getNext();
            Error error;
            if (action_id == -1) return Error.ACTION_TOTAL_MAX_REACHED;

            ActionRecord record = ActionFactory.getActionRequirementRecord(workerType);

            if (record == null) return Error.ACTION_NOT_FOUND;

            List<ActiveAction> stubs_remain = active.FindAll(delegate(ActiveAction stub) {
                return stub.WorkerObject == workerObject;
            });

            if (stubs_remain.Count >= record.max) return Error.ACTION_WORKER_MAX_REACHED;

            foreach (ActionRequirement action_req in record.list) {
                if (action_req.type == action.Type) {
                    if ((error = action.validate(action_req.parms)) == Error.OK) {
                        if (stubs_remain.FindAll(delegate(ActiveAction stub) {
                            return stub.WorkerIndex == action_req.index;
                        }).Count >= action_req.max) {
                            return Error.ACTION_INDEX_MAX_REACHED;
                        }

                        if ((error = EffectRequirementFactory.getEffectRequirementContainer(action_req.effect_req_id).validate(workerObject, effects.GetAllEffects(action_req.effect_req_inherit))) == Error.OK) {
                            action.OnNotify += this.notifyActive;
                            action.ActionId = (ushort)action_id;
                            action.WorkerIndex = action_req.index;
                            action.WorkerType = workerType;
                            action.WorkerObject = workerObject;
                            active.Add(action.ActionId, action);

                            Error ret = action.execute();

                            if (ret != Error.OK) {
                                action.stateChange(ActionState.FAILED);
                                active.Remove(action.ActionId);
                                releaseId(action.ActionId);
                            } else {
                                action.stateChange(ActionState.STARTED);
                            }

                            return ret;
                        } else {
                            Global.Logger.Debug("Effect Requirement failed for action[" + action.Type.ToString() + "].");
                            return error;
                        }
                    } else if (error != Error.ACTION_INVALID) {
                        return error;
                    }
                }
            }

            return Error.ACTION_INVALID;
        }

        public Error doPassive(ICanDo workerObject, PassiveAction action, bool visible) {
            action.IsVisible = visible;

            int action_id = actionIdGen.getNext();
            if (action_id == -1) return Error.ACTION_TOTAL_MAX_REACHED;

            action.OnNotify += this.notifyPassive;
            action.WorkerObject = workerObject;
            action.ActionId = (ushort)action_id;

            passive.Add(action.ActionId, action);

            Error ret = action.execute();
            if (ret != Error.OK) {
                action.stateChange(ActionState.FAILED);
                active.Remove(action.ActionId);
                releaseId(action.ActionId);
            } else {
                action.stateChange(ActionState.STARTED);
            }

            return ret;
        }

        public void doOnce(GameObject workerObject, PassiveAction action) {
            if (passive.Exists(delegate(PassiveAction a) {
                return a.Type == action.Type;
            })) return;

            int action_id = actionIdGen.getNext();
            if (action_id == -1) throw new Exception(Error.ACTION_TOTAL_MAX_REACHED.ToString());

            action.WorkerObject = workerObject;
            action.OnNotify += this.notifyPassive;
            action.ActionId = (ushort)action_id;
            passive.Add(action.ActionId, action);

            using (new MultiObjectLock(city)) {
                action.execute();
            }
        }
        #endregion

        #region Methods
        public bool contains(Action action) {
            if (action is ActiveAction)
                return ActiveActions.ContainsKey(action.ActionId);
            else if (action is PassiveAction)
                return PassiveActions.ContainsKey(action.ActionId);
            else
                return false;
        }

        public void fireCallback(ISchedule action) {
            if (!(action as Action).IsDone) {
                action.callback(null);
            }
        }

        internal IEnumerable<Action> getVisibleActions() {
            foreach (KeyValuePair<ushort, ActiveAction> kvp in active)
                yield return kvp.Value;

            foreach (KeyValuePair<ushort, PassiveAction> kvp in passive) {
                if (kvp.Value.IsVisible)
                    yield return kvp.Value;
            }
        }

        public int getId() {
            int actionId = actionIdGen.getNext();
            if (actionId == -1) throw new Exception(Error.ACTION_TOTAL_MAX_REACHED.ToString());

            return actionId;
        }

        public void releaseId(int actionId) {
            actionIdGen.release(actionId);
        }

        void schedule(ScheduledActiveAction action) {
            ActionDispatcher dispatcher = new ActionDispatcher(action);
            Global.Scheduler.put(dispatcher);
        }

        void schedule(ScheduledPassiveAction action) {
            ActionDispatcher dispatcher = new ActionDispatcher(action);
            Global.Scheduler.put(dispatcher);
        }
        #endregion

        #region Event
        public delegate void UpdateCallback(Action stub);
        public event UpdateCallback ActionStarted;
        public event UpdateCallback ActionRescheduled;
        public event UpdateCallback ActionRemoved;
        #endregion

        public void Remove(GameObject workerObject, ActionInterrupt actionInterrupt, params Action[] ignoreActions) {
            List<Action> ignoreActionList = new List<Action>(ignoreActions);

            List<ActiveAction> list = active.FindAll(delegate(ActiveAction action_stub) {
                return action_stub.WorkerObject == workerObject;
            });

            foreach (ActiveAction stub in list) {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.interrupt(actionInterrupt);
                active.Remove(stub.ActionId);
                Global.dbManager.Delete(stub);
            }

            List<PassiveAction> full_list = passive.FindAll(delegate(PassiveAction action) {
                return action.WorkerObject == workerObject;
            });

            foreach (PassiveAction stub in full_list) {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.interrupt(actionInterrupt);
                passive.Remove(stub.ActionId);
                Global.dbManager.Delete(stub);
            }
        }

        private void CancelCallback(object item) {
            ((ActiveAction)item).interrupt(ActionInterrupt.CANCEL);
        }
        public Error Cancel(ushort id) {
            ActiveAction action;
            if (ActiveActions.TryGetValue(id, out action) && !action.isDone) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(CancelCallback), action);
                    return Error.OK;
        
            }
            else {
                return Error.ACTION_NOT_FOUND;
            }
        }

        #region ISchedule Members

        public DateTime Time {
            get { throw new NotImplementedException(); }
        }

        public void callback(object custom) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
