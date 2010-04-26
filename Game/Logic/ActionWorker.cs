#region

using System;
using System.Collections.Generic;
using System.Threading;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic {
    public class ActionRequirement {
        public byte index;
        public byte max;
        public ActionType type;
        public string[] parms;
        public uint effectReqId;
        public EffectInheritance effectReqInherit;
    }

    public class ActionWorker {
        private readonly LargeIdGenerator actionIdGen = new LargeIdGenerator(ushort.MaxValue);

        private readonly ListDictionary<ushort, ActiveAction> active = new ListDictionary<ushort, ActiveAction>();
        private readonly ListDictionary<ushort, PassiveAction> passive = new ListDictionary<ushort, PassiveAction>();

        private readonly NotificationManager notifications;
        private readonly ReferenceManager references;

        private readonly City city;

        public ActionWorker(City owner) {
            city = owner;
            notifications = new NotificationManager(this);
            references = new ReferenceManager(this);
        }

        #region Properties

        public byte Count {
            get { return (byte) active.Count; }
        }

        public ListDictionary<ushort, ActiveAction> ActiveActions {
            get { return active; }
        }

        public ListDictionary<ushort, PassiveAction> PassiveActions {
            get { return passive; }
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

        private void NotifyPassive(GameAction action, ActionState state) {
            MultiObjectLock.ThrowExceptionIfNotLocked(city);

            PassiveAction actionStub;
            if (!passive.TryGetValue(action.ActionId, out actionStub))
                return;

            switch (state) {
                case ActionState.RESCHEDULED:
                    if (ActionRescheduled != null)
                        ActionRescheduled(actionStub);

                    if (action is PassiveAction)
                        Global.DbManager.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.STARTED:
                    if (ActionStarted != null)
                        ActionStarted(actionStub);

                    if (action is PassiveAction)
                        Global.DbManager.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.COMPLETED:
                    passive.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub);

                    if (action is PassiveAction)
                        Global.DbManager.Delete(actionStub);
                    break;
                case ActionState.FIRED:
                    if (action is PassiveAction)
                        Global.DbManager.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.INTERRUPTED:
                case ActionState.FAILED:
                    passive.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub);

                    if (action is PassiveAction)
                        Global.DbManager.Delete(actionStub);
                    break;
            }
        }

        private void NotifyActive(GameAction action, ActionState state) {
            MultiObjectLock.ThrowExceptionIfNotLocked(city);

            ActiveAction actionStub;
            if (!active.TryGetValue(action.ActionId, out actionStub))
                return;

            switch (state) {
                case ActionState.RESCHEDULED:
                    if (ActionRescheduled != null)
                        ActionRescheduled(actionStub);

                    if (actionStub is ScheduledActiveAction) {
                        Global.DbManager.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.STARTED:
                    if (ActionStarted != null)
                        ActionStarted(actionStub);

                    if (action is ScheduledActiveAction) {
                        Global.DbManager.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.COMPLETED:
                    active.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub);

                    if (action is ScheduledActiveAction)
                        Global.DbManager.Delete(actionStub);
                    break;
                case ActionState.FIRED:
                    if (action is ScheduledActiveAction) {
                        Global.DbManager.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.INTERRUPTED:
                case ActionState.FAILED:
                    active.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub);

                    if (action is ScheduledActiveAction)
                        Global.DbManager.Delete(actionStub);
                    break;
            }
        }

        #endregion

        #region DB Loader

        public void DbLoaderDoActive(ActiveAction action) {
            //this should only be used by the db loader
            if (action is ScheduledActiveAction)
                Schedule(action as ScheduledActiveAction);

            action.OnNotify += NotifyActive;
            actionIdGen.set(action.ActionId);
            active.Add(action.ActionId, action);
        }

        public void DbLoaderDoPassive(PassiveAction action) {
            //this should only be used by the db loader
            if (action is ScheduledPassiveAction)
                Schedule(action as ScheduledPassiveAction);

            action.OnNotify += NotifyPassive;
            actionIdGen.set(action.ActionId);
            passive.Add(action.ActionId, action);
        }

        #endregion

        #region Scheduling

        public Error DoActive(int workerType, GameObject workerObject, ActiveAction action, IHasEffect effects) {
            int actionId = actionIdGen.getNext();
            Error error;
            if (actionId == -1)
                return Error.ACTION_TOTAL_MAX_REACHED;

            ActionRecord record = ActionFactory.getActionRequirementRecord(workerType);

            if (record == null)
                return Error.ACTION_NOT_FOUND;

            List<ActiveAction> stubsRemain =
                active.FindAll(stub => stub.WorkerObject == workerObject);

            if (stubsRemain.Count >= record.max)
                return Error.ACTION_WORKER_MAX_REACHED;

            foreach (ActionRequirement actionReq in record.list) {
                if (actionReq.type != action.Type)
                    continue;

                if ((error = action.Validate(actionReq.parms)) == Error.OK) {
                    if (stubsRemain.FindAll(stub => stub.WorkerIndex == actionReq.index).Count >= actionReq.max) {
                        return Error.ACTION_INDEX_MAX_REACHED;
                    }

                    if ((error = EffectRequirementFactory.getEffectRequirementContainer(actionReq.effectReqId).validate(workerObject, effects.GetAllEffects(actionReq.effectReqInherit))) ==
                        Error.OK) {
                        action.OnNotify += NotifyActive;
                        action.ActionId = (ushort) actionId;
                        action.WorkerIndex = actionReq.index;
                        action.WorkerType = workerType;
                        action.WorkerObject = workerObject;
                        active.Add(action.ActionId, action);

                        Error ret = action.Execute();

                        if (ret != Error.OK) {
                            action.StateChange(ActionState.FAILED);
                            active.Remove(action.ActionId);
                            ReleaseId(action.ActionId);
                        } else
                            action.StateChange(ActionState.STARTED);

                        return ret;
                    }

                    Global.Logger.Debug("Effect Requirement failed for action[" + action.Type + "].");
                    return error;
                }

                if (error != Error.ACTION_INVALID)
                    return error;
            }

            return Error.ACTION_INVALID;
        }

        public Error DoPassive(ICanDo workerObject, PassiveAction action, bool visible) {
            action.IsVisible = visible;

            int actionId = actionIdGen.getNext();
            if (actionId == -1)
                return Error.ACTION_TOTAL_MAX_REACHED;

            action.OnNotify += NotifyPassive;
            action.WorkerObject = workerObject;
            action.ActionId = (ushort) actionId;

            passive.Add(action.ActionId, action);

            Error ret = action.Execute();
            if (ret != Error.OK) {
                action.StateChange(ActionState.FAILED);
                active.Remove(action.ActionId);
                ReleaseId(action.ActionId);
            } else
                action.StateChange(ActionState.STARTED);

            return ret;
        }

        public void DoOnce(GameObject workerObject, PassiveAction action) {
            if (passive.Exists(a => a.Type == action.Type))
                return;

            int actionId = actionIdGen.getNext();
            if (actionId == -1)
                throw new Exception(Error.ACTION_TOTAL_MAX_REACHED.ToString());

            action.WorkerObject = workerObject;
            action.OnNotify += NotifyPassive;
            action.ActionId = (ushort) actionId;
            passive.Add(action.ActionId, action);

            using (new MultiObjectLock(city))
                action.Execute();
        }

        #endregion

        #region Methods

        public bool Contains(GameAction action) {
            if (action is ActiveAction)
                return ActiveActions.ContainsKey(action.ActionId);
            
            if (action is PassiveAction)
                return PassiveActions.ContainsKey(action.ActionId);
            
            return false;
        }

        public void FireCallback(ISchedule action) {
            if (!((GameAction) action).IsDone)
                action.Callback(null);
        }

        internal IEnumerable<GameAction> GetVisibleActions() {
            foreach (KeyValuePair<ushort, ActiveAction> kvp in active)
                yield return kvp.Value;

            foreach (KeyValuePair<ushort, PassiveAction> kvp in passive) {
                if (kvp.Value.IsVisible)
                    yield return kvp.Value;
            }
        }

        public int GetId() {
            int actionId = actionIdGen.getNext();
            if (actionId == -1)
                throw new Exception(Error.ACTION_TOTAL_MAX_REACHED.ToString());

            return actionId;
        }

        public void ReleaseId(int actionId) {
            actionIdGen.release(actionId);
        }

        private static void Schedule(ScheduledActiveAction action) {
            ActionDispatcher dispatcher = new ActionDispatcher(action);
            Global.Scheduler.Put(dispatcher);
        }

        private static void Schedule(ScheduledPassiveAction action) {
            ActionDispatcher dispatcher = new ActionDispatcher(action);
            Global.Scheduler.Put(dispatcher);
        }

        #endregion

        #region Event

        public delegate void UpdateCallback(GameAction stub);

        public event UpdateCallback ActionStarted;
        public event UpdateCallback ActionRescheduled;
        public event UpdateCallback ActionRemoved;

        #endregion

        public void Remove(GameObject workerObject, ActionInterrupt actionInterrupt, params GameAction[] ignoreActions) {
            MultiObjectLock.ThrowExceptionIfNotLocked(city);

            references.Remove(workerObject);

            List<GameAction> ignoreActionList = new List<GameAction>(ignoreActions);

            List<ActiveAction> list =
                active.FindAll(actionStub => actionStub.WorkerObject == workerObject);

            foreach (ActiveAction stub in list) {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.Interrupt(actionInterrupt);
                active.Remove(stub.ActionId);
                Global.DbManager.Delete(stub);
            }

            List<PassiveAction> fullList =
                passive.FindAll(action => action.WorkerObject == workerObject);

            foreach (PassiveAction stub in fullList) {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.Interrupt(actionInterrupt);
                passive.Remove(stub.ActionId);
                Global.DbManager.Delete(stub);
            }
        }

        private static void CancelCallback(object item) {
            ((ActiveAction) item).Interrupt(ActionInterrupt.CANCEL);
        }

        public Error Cancel(ushort id) {
            ActiveAction action;
            if (ActiveActions.TryGetValue(id, out action) && !action.isDone) {
                ThreadPool.QueueUserWorkItem(CancelCallback, action);
                return Error.OK;
            }

            return Error.ACTION_NOT_FOUND;
        }
    }
}