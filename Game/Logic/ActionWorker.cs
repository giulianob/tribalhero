#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Data;
using Game.Database;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic
{
    public class ActionWorker : IActionWorker
    {
        public ILocation Location { get; set; }

        private readonly Func<ILockable> lockDelegate;

        private readonly LargeIdGenerator actionIdGen = new LargeIdGenerator(ushort.MaxValue);

        private readonly IDictionary<uint, ActiveAction> active = new Dictionary<uint, ActiveAction>();        

        private readonly IDictionary<uint, PassiveAction> passive = new Dictionary<uint, PassiveAction>();

        public ActionWorker(Func<ILockable> lockDelegate, ILocation location)
        {
            Location = location;
            this.lockDelegate = lockDelegate;
        }

        #region Properties

        public IDictionary<uint, ActiveAction> ActiveActions
        {
            get
            {
                return active;
            }
        }

        public IDictionary<uint, PassiveAction> PassiveActions
        {
            get
            {
                return passive;
            }
        }
        
        #endregion

        private void NotifyPassive(GameAction action, ActionState state)
        {
            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(lockDelegate());

            PassiveAction actionStub;
            if (!passive.TryGetValue(action.ActionId, out actionStub))
                return;

            switch(state)
            {
                case ActionState.Rescheduled:
                    if (ActionRescheduled != null)
                        ActionRescheduled(actionStub, state);

                    if (action is PassiveAction)
                        DbPersistance.Current.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.Started:
                    if (ActionStarted != null)
                        ActionStarted(actionStub, state);

                    if (action is PassiveAction)
                        DbPersistance.Current.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.Completed:
                    passive.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);                    

                    if (action is ScheduledPassiveAction)
                        Scheduler.Current.Remove(action as ScheduledPassiveAction);

                    if (action is PassiveAction)
                        DbPersistance.Current.Delete(actionStub);
                    break;
                case ActionState.Fired:
                    if (action is PassiveAction)
                        DbPersistance.Current.Save(actionStub);

                    if (action is ScheduledPassiveAction)
                        Schedule(action as ScheduledPassiveAction);
                    break;
                case ActionState.Failed:
                    passive.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);
                    
                    if (action is ScheduledPassiveAction)
                        Scheduler.Current.Remove(action as ScheduledPassiveAction);

                    if (action is PassiveAction)
                        DbPersistance.Current.Delete(actionStub);
                    break;
            }
        }

        private void NotifyActive(GameAction action, ActionState state)
        {
            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(lockDelegate());

            ActiveAction actionStub;
            if (!active.TryGetValue(action.ActionId, out actionStub))
                return;

            switch(state)
            {
                case ActionState.Rescheduled:
                    if (ActionRescheduled != null)
                        ActionRescheduled(actionStub, state);

                    if (actionStub is ScheduledActiveAction)
                    {
                        DbPersistance.Current.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.Started:
                    if (ActionStarted != null)
                        ActionStarted(actionStub, state);

                    if (action is ScheduledActiveAction)
                    {
                        DbPersistance.Current.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.Completed:
                    active.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);

                    if (action is ScheduledActiveAction)
                    {
                        DbPersistance.Current.Delete(actionStub);
                        Scheduler.Current.Remove(action as ISchedule);
                    }
                    break;
                case ActionState.Fired:
                    if (action is ScheduledActiveAction)
                    {
                        DbPersistance.Current.Save(actionStub);
                        Schedule(action as ScheduledActiveAction);
                    }
                    break;
                case ActionState.Failed:
                    active.Remove(actionStub.ActionId);
                    action.IsDone = true;

                    if (ActionRemoved != null)
                        ActionRemoved(actionStub, state);

                    if (action is ScheduledActiveAction)
                    {
                        DbPersistance.Current.Delete(actionStub);
                        Scheduler.Current.Remove(action as ISchedule);
                    }
                    break;
            }
        }

        public PassiveAction FindAction(IGameObject workerObject, Type type)
        {
            return passive.Values.FirstOrDefault(action => action.WorkerObject == workerObject && action.GetType() == type);
        }

        public void Remove(IGameObject workerObject, params GameAction[] ignoreActions)
        {
            var ignoreActionList = new List<GameAction>(ignoreActions);

            // Cancel Active actions
            IList<ActiveAction> activeList;
            using (Concurrency.Current.Lock(lockDelegate()))
            {
                // Very important to keep the ToList() here since we will be modifying the collection in the loop below and an IEnumerable will crash!
                activeList = active.Values.Where(actionStub => actionStub.WorkerObject == workerObject).ToList();
            }

            foreach (var stub in activeList)
            {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.WorkerRemoved(false);
            }

            // Cancel Passive actions
            IList<PassiveAction> passiveList;
            using (Concurrency.Current.Lock(lockDelegate()))
            {
                // Very important to keep the ToList() here since we will be modifying the collection in the loop below and an IEnumerable will crash!
                passiveList = passive.Values.Where(action => action.WorkerObject == workerObject).ToList();
            }

            foreach (var stub in passiveList)
            {
                if (ignoreActionList.Contains(stub))
                    continue;

                stub.WorkerRemoved(false);
            }

            ActionsRemovedFromWorker(workerObject);
        }

        private static void ActiveCancelCallback(object item)
        {
            ((ActiveAction)item).UserCancelled();
        }

        private static void PassiveCancelCallback(object item)
        {
            ((PassiveAction)item).UserCancelled();
        }

        public Error Cancel(uint id)
        {
            ActiveAction activeAction;
            if (ActiveActions.TryGetValue(id, out activeAction) && !activeAction.IsDone)
            {
                var actionRequirements = Ioc.Kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(activeAction.WorkerType);
                var actionRequirement = actionRequirements.List.FirstOrDefault(x => x.Index == activeAction.WorkerIndex);
                if (actionRequirement == null || (actionRequirement.Option & ActionOption.Uncancelable) == ActionOption.Uncancelable)
                    return Error.ActionUncancelable;

                ThreadPool.QueueUserWorkItem(ActiveCancelCallback, activeAction);
                return Error.Ok;
            }

            PassiveAction passiveAction;
            if (PassiveActions.TryGetValue(id, out passiveAction) && !passiveAction.IsDone)
            {
                if (!passiveAction.IsCancellable)
                    return Error.ActionUncancelable;

                ThreadPool.QueueUserWorkItem(PassiveCancelCallback, passiveAction);
                return Error.Ok;
            }

            return Error.ActionNotFound;
        }

        #region DB Loader

        public void DbLoaderDoActive(ActiveAction action)
        {
            //this should only be used by the db loader
            if (action is ScheduledActiveAction)
                Schedule(action as ScheduledActiveAction);

            SetActiveActionParams(action);

            actionIdGen.Set(action.ActionId);
            active.Add(action.ActionId, action);
        }

        public void DbLoaderDoPassive(PassiveAction action)
        {
            SetPassiveActionParams(action);

            if (action.IsChain)
            {
                action.OnNotify -= NotifyPassive;
                actionIdGen.Set(action.ActionId);
            }
            else
            {
                if (action is ScheduledPassiveAction)
                {
                    Schedule(action as ScheduledPassiveAction);
                }
                
                actionIdGen.Set(action.ActionId);
                passive.Add(action.ActionId, action);
            }
        }

        #endregion

        #region Scheduling

        public Error DoActive(int workerType, IGameObject workerObject, ActiveAction action, IHasEffect effects)
        {
            if (workerObject.IsBlocked)
                return Error.ObjectNotFound;

            int actionId = actionIdGen.GetNext();

            if (actionId == -1)
                return Error.ActionTotalMaxReached;

            ActionRequirementFactory.ActionRecord record = Ioc.Kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(workerType);

            if (record == null)
                return Error.ActionNotFound;

            foreach (var actionReq in record.List.Where(x => x.Type == action.Type))
            {
                var error = action.Validate(actionReq.Parms);

                if (error != Error.Ok)
                {
                    if (error != Error.ActionInvalid)
                        return error;

                    continue;
                }

                if (!CanDoActiveAction(action, actionReq, workerObject))
                    return Error.ActionTotalMaxReached;

                error = Ioc.Kernel.Get<EffectRequirementFactory>().GetEffectRequirementContainer(actionReq.EffectReqId).Validate(workerObject,
                                                                                                               effects.GetAllEffects(actionReq.EffectReqInherit));

                if (error != Error.Ok)
                    return error;

                SetActiveActionParams(action);

                action.ActionId = (ushort)actionId;
                action.WorkerIndex = actionReq.Index;
                action.WorkerType = workerType;
                action.WorkerObject = workerObject;                
                active.Add(action.ActionId, action);

                Error ret = action.Execute();

                if (ret != Error.Ok)
                {
                    action.StateChange(ActionState.Failed);
                    active.Remove(action.ActionId);
                    ReleaseId(action.ActionId);
                }
                else
                    action.StateChange(ActionState.Started);

                return ret;
            }

            return Error.ActionInvalid;
        }

        private bool CanDoActiveAction(ActiveAction action, ActionRequirement actionReq, IGameObject worker)
        {
            switch(action.ActionConcurrency)
            {
                case ConcurrencyType.StandAlone:
                    return active.All(x => x.Value.WorkerObject != worker);
                case ConcurrencyType.Normal:
                    return !active.Any(x => x.Value.WorkerObject == worker && (x.Value.ActionConcurrency == ConcurrencyType.StandAlone || x.Value.ActionConcurrency == ConcurrencyType.Normal));
                case ConcurrencyType.Concurrent:
                    return !active.Any(x => x.Value.WorkerObject == worker && (x.Value.Type == actionReq.Type || x.Value.ActionConcurrency == ConcurrencyType.StandAlone));
            }

            return false;
        }

        public Error DoPassive(ICanDo workerObject, PassiveAction action, bool visible)
        {
            if (workerObject.IsBlocked)
                return Error.ObjectNotFound;

            action.IsVisible = visible;

            int actionId = actionIdGen.GetNext();
            if (actionId == -1)
                return Error.ActionTotalMaxReached;

            SetPassiveActionParams(action);

            action.WorkerObject = workerObject;
            action.ActionId = (uint)actionId;

            passive.Add(action.ActionId, action);

            Error ret = action.Execute();
            if (ret != Error.Ok)
            {
                action.StateChange(ActionState.Failed);
                passive.Remove(action.ActionId);
                ReleaseId(action.ActionId);
            }
            else
                action.StateChange(ActionState.Started);

            return ret;
        }

        private void SetActiveActionParams(ActiveAction action)
        {
            action.Location = Location;
            action.OnNotify += NotifyActive;
        }

        private void SetPassiveActionParams(PassiveAction action)
        {
            action.OnNotify += NotifyPassive;            
            action.ActionIdGenerator = actionIdGen;            
            action.Location = Location;
        }

        public void DoOnce(IGameObject workerObject, PassiveAction action)
        {
            if (passive.Values.Any(a => a.Type == action.Type))
                return;

            int actionId = actionIdGen.GetNext();
            if (actionId == -1)
                throw new Exception(Error.ActionTotalMaxReached.ToString());

            action.WorkerObject = workerObject;            
            action.ActionId = (ushort)actionId;

            SetPassiveActionParams(action);

            passive.Add(action.ActionId, action);

            using (Concurrency.Current.Lock(lockDelegate()))
            {
                action.Execute();
            }
        }

        #endregion

        #region Methods

        public IEnumerable<GameAction> GetActions(IGameObject gameObject)
        {
            var actions = new List<GameAction>();

            actions.AddRange(PassiveActions.Values.Where(x => x.WorkerObject == gameObject));
            actions.AddRange(ActiveActions.Values.Where(x => x.WorkerObject == gameObject));

            return actions;
        }

        public bool Contains(GameAction action)
        {
            if (action is ActiveAction)
                return ActiveActions.ContainsKey(action.ActionId);

            if (action is PassiveAction)
                return PassiveActions.ContainsKey(action.ActionId);

            return false;
        }

        public bool Contains(ActionType type, uint ignoreId)
        {
            if (ActiveActions.Values.Any(x => x.Type == type && x.ActionId != ignoreId))
                return true;

            if (PassiveActions.Values.Any(x => x.Type == type && x.ActionId != ignoreId))
                return true;

            return false;
        }

        public IEnumerable<GameAction> GetVisibleActions()
        {
            foreach (var kvp in active)
                yield return kvp.Value;

            foreach (var kvp in passive)
            {
                if (kvp.Value.IsVisible)
                    yield return kvp.Value;
            }
        }

        public int GetId()
        {
            int actionId = actionIdGen.GetNext();
            if (actionId == -1)
                throw new Exception(Error.ActionTotalMaxReached.ToString());

            return actionId;
        }

        public void ReleaseId(uint actionId)
        {
            actionIdGen.Release(actionId);
        }

        private static void Schedule(ScheduledActiveAction action)
        {
            Scheduler.Current.Put(action);
        }

        private static void Schedule(ScheduledPassiveAction action)
        {
            Scheduler.Current.Put(action);
        }

        #endregion

        #region Event

        #region Delegates

        public delegate void UpdateCallback(GameAction stub, ActionState state);

        public delegate void RemovedWorkerCallback(IGameObject workerObject);

        #endregion

        public event UpdateCallback ActionStarted;
        public event UpdateCallback ActionRescheduled;
        public event UpdateCallback ActionRemoved;
        public event RemovedWorkerCallback ActionsRemovedFromWorker;

        #endregion
    }
}