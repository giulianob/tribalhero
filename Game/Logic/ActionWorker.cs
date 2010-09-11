#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic {
    public class ActionRequirement {
        public byte index;
        public byte max;
        public ActionType type;
        public ActionOption option;
        public string[] parms;
        public uint effectReqId;
        public EffectInheritance effectReqInherit;
    }

    public class ActionWorker {
        private readonly LargeIdGenerator actionIdGen = new LargeIdGenerator(ushort.MaxValue);

        private readonly ListDictionary<uint, ActiveAction> active = new ListDictionary<uint, ActiveAction>();
        private readonly ListDictionary<uint, PassiveAction> passive = new ListDictionary<uint, PassiveAction>();

        private readonly NotificationManager notifications;
        private readonly ReferenceManager references;

        private readonly City city;

        public ActionWorker(City owner) {
            city = owner;
            notifications = new NotificationManager(this);
            references = new ReferenceManager(this);
        }

        #region Properties
        public ListDictionary<uint, ActiveAction> ActiveActions {
            get { return active; }
        }

        public ListDictionary<uint, PassiveAction> PassiveActions {
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
                        ActionRescheduled(actionStub, state);

                    if (action is PassiveAction)
                        Global.DbManager.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.STARTED:
                    if (ActionStarted != null)
                        ActionStarted(actionStub, state);

                    if (action is PassiveAction)
                        Global.DbManager.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.COMPLETED:
                    passive.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);

                    if (action is ScheduledPassiveAction)
                        Global.Scheduler.Del(action as ScheduledPassiveAction);

                    if (action is PassiveAction)
                        Global.DbManager.Delete(actionStub);
                    break;
                case ActionState.FIRED:
                    if (action is PassiveAction)
                        Global.DbManager.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.FAILED:
                    passive.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);

                    if (action is ScheduledPassiveAction)
                        Global.Scheduler.Del(action as ScheduledPassiveAction);

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
                        ActionRescheduled(actionStub, state);

                    if (actionStub is ScheduledActiveAction) {
                        Global.DbManager.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.STARTED:
                    if (ActionStarted != null)
                        ActionStarted(actionStub, state);

                    if (action is ScheduledActiveAction) {
                        Global.DbManager.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.COMPLETED:
                    active.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);

                    if (action is ScheduledActiveAction) {
                        Global.DbManager.Delete(actionStub);
                        Global.Scheduler.Del(action as ISchedule);
                    }
                    break;
                case ActionState.FIRED:
                    if (action is ScheduledActiveAction) {
                        Global.DbManager.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;                
                case ActionState.FAILED:                       
                    active.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);

                    if (action is ScheduledActiveAction) {
                        Global.DbManager.Delete(actionStub);
                        Global.Scheduler.Del(action as ISchedule);
                    }
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
            actionIdGen.Set(action.ActionId);
            active.Add(action.ActionId, action);
        }

        public void DbLoaderDoPassive(PassiveAction action) {

            if (action.IsChain) {
                actionIdGen.Set(action.ActionId);
            }
            else {
                //this should only be used by the db loader
                if (action is ScheduledPassiveAction)
                    Schedule(action as ScheduledPassiveAction);

                action.OnNotify += NotifyPassive;
                actionIdGen.Set(action.ActionId);
                passive.Add(action.ActionId, action);
            }
        }

        #endregion

        #region Scheduling

        public Error DoActive(int workerType, GameObject workerObject, ActiveAction action, IHasEffect effects) {
            
            if (workerObject.IsBlocked)
                return Error.OBJECT_NOT_FOUND;

            int actionId = actionIdGen.GetNext();

            Error error;
            if (actionId == -1)
                return Error.ACTION_TOTAL_MAX_REACHED;

            ActionRecord record = ActionFactory.GetActionRequirementRecord(workerType);

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

            if (workerObject is GameObject && ((GameObject)workerObject).IsBlocked)
                return Error.OBJECT_NOT_FOUND;

            action.IsVisible = visible;

            int actionId = actionIdGen.GetNext();
            if (actionId == -1)
                return Error.ACTION_TOTAL_MAX_REACHED;

            action.OnNotify += NotifyPassive;
            action.WorkerObject = workerObject;
            action.ActionId = (uint) actionId;

            passive.Add(action.ActionId, action);

            Error ret = action.Execute();
            if (ret != Error.OK) {
                action.StateChange(ActionState.FAILED);
                passive.Remove(action.ActionId);
                ReleaseId(action.ActionId);
            } else
                action.StateChange(ActionState.STARTED);

            return ret;
        }

        public void DoOnce(GameObject workerObject, PassiveAction action) {
            if (passive.Exists(a => a.Type == action.Type))
                return;

            int actionId = actionIdGen.GetNext();
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

        public IEnumerable<GameAction> GetActions(GameObject gameObject) {
            List<GameAction> actions = new List<GameAction>();

            actions.AddRange(PassiveActions.FindAll(x => x.WorkerObject == gameObject).Cast<GameAction>());
            actions.AddRange(ActiveActions.FindAll(x => x.WorkerObject == gameObject).Cast<GameAction>());

            return actions;
        }

        public bool Contains(GameAction action) {
            if (action is ActiveAction)
                return ActiveActions.ContainsKey(action.ActionId);
            
            if (action is PassiveAction)
                return PassiveActions.ContainsKey(action.ActionId);
            
            return false;
        }

        public void FireCallback(ISchedule action) {
            if (!((GameAction)action).IsDone) {
                action.Callback(null);
            }
        }

        internal IEnumerable<GameAction> GetVisibleActions() {
            foreach (KeyValuePair<uint, ActiveAction> kvp in active)
                yield return kvp.Value;

            foreach (KeyValuePair<uint, PassiveAction> kvp in passive) {
                if (kvp.Value.IsVisible)
                    yield return kvp.Value;
            }
        }

        public int GetId() {
            int actionId = actionIdGen.GetNext();
            if (actionId == -1)
                throw new Exception(Error.ACTION_TOTAL_MAX_REACHED.ToString());

            return actionId;
        }

        public void ReleaseId(uint actionId) {
            actionIdGen.Release(actionId);
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

        public delegate void UpdateCallback(GameAction stub, ActionState state);

        public event UpdateCallback ActionStarted;
        public event UpdateCallback ActionRescheduled;
        public event UpdateCallback ActionRemoved;

        #endregion

        public PassiveAction FindAction(GameObject workerObject, Type type) {
            return passive.Values.FirstOrDefault(action => action.WorkerObject == workerObject && action.GetType() == type);
        }

        public void Remove(GameObject workerObject, ActionInterrupt actionInterrupt, params GameAction[] ignoreActions) {
            List<GameAction> ignoreActionList = new List<GameAction>(ignoreActions);
                        
            // Cancel Active actions
            List<ActiveAction> activeList;
            using (new MultiObjectLock(City)) {
                activeList = active.FindAll(actionStub => actionStub.WorkerObject == workerObject);
            }

            foreach (ActiveAction stub in activeList) {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.UserCancelled();
            }

            // Cancel Passive actions
            List<PassiveAction> passiveList;
            using (new MultiObjectLock(City)) {
                passiveList = passive.FindAll(action => action.WorkerObject == workerObject);
            }

            foreach (PassiveAction stub in passiveList) {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.UserCancelled();
            }

            using (new MultiObjectLock(City)) {
                references.Remove(workerObject);
            }
        }

        public void DoInterrupt(ushort actionId, bool wasKilled) {            
            ActiveAction activeAction;
            if (ActiveActions.TryGetValue(actionId, out activeAction))
            {
                activeAction.WorkerRemoved(wasKilled);
                return;
            }

            PassiveAction passiveAction;
            if (PassiveActions.TryGetValue(actionId, out passiveAction)) {
                passiveAction.WorkerRemoved(wasKilled);
                return;
            }
        }

        private static void ActiveCancelCallback(object item) {
            ((ActiveAction) item).UserCancelled();
        }

        private static void PassiveCancelCallback(object item) {
            ((PassiveAction)item).UserCancelled();
        }        

        public Error Cancel(ushort id) {
            ActiveAction activeAction;
            if (ActiveActions.TryGetValue(id, out activeAction) && !activeAction.isDone) {
                if ((ActionFactory.GetActionRequirementRecord(activeAction.WorkerType).list[activeAction.WorkerIndex-1].option & ActionOption.UNCANCELABLE) == ActionOption.UNCANCELABLE) {
                    return Error.ACTION_UNCANCELABLE;
                }
                ThreadPool.QueueUserWorkItem(ActiveCancelCallback, activeAction);
                return Error.OK;
            }

            PassiveAction passiveAction;
            if (PassiveActions.TryGetValue(id, out passiveAction) && !passiveAction.isDone) {
                if (!passiveAction.isCancellable)
                    return Error.ACTION_UNCANCELABLE;

                ThreadPool.QueueUserWorkItem(PassiveCancelCallback, passiveAction);
                return Error.OK;
            }

            return Error.ACTION_NOT_FOUND;
        }
    }
}