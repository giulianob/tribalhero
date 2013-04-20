using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util.Locking;

namespace Game.Logic
{
    public interface IActionWorker
    {
        ILocation Location { get; set; }

        Func<ILockable> LockDelegate { get; set; }

        IDictionary<uint, ActiveAction> ActiveActions { get; }

        IDictionary<uint, PassiveAction> PassiveActions { get; }

        PassiveAction FindAction(IGameObject workerObject, Type type);

        void Remove(IGameObject workerObject, params GameAction[] ignoreActions);

        Error Cancel(uint id);

        void DbLoaderDoActive(ActiveAction action);

        void DbLoaderDoPassive(PassiveAction action);

        Error DoActive(int workerType, IGameObject workerObject, ActiveAction action, IHasEffect effects);

        Error DoPassive(ICanDo workerObject, PassiveAction action, bool visible);

        void DoOnce(IGameObject workerObject, PassiveAction action);

        IEnumerable<GameAction> GetActions(IGameObject gameObject);

        bool Contains(GameAction action);

        bool Contains(ActionType type, uint ignoreId);

        IEnumerable<GameAction> GetVisibleActions();

        uint GetId();

        event ActionWorker.UpdateCallback ActionStarted;

        event ActionWorker.UpdateCallback ActionRescheduled;

        event ActionWorker.UpdateCallback ActionRemoved;

        event ActionWorker.RemovedWorkerCallback ActionsRemovedFromWorker;
    }
}