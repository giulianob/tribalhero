using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;

namespace Game.Logic
{
    public interface IActionWorker
    {
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

        int GetId();

        void ReleaseId(uint actionId);

        event ActionWorker.UpdateCallback ActionStarted;

        event ActionWorker.UpdateCallback ActionRescheduled;

        event ActionWorker.UpdateCallback ActionRemoved;

        event ActionWorker.RemovedWorkerCallback ActionsRemovedFromWorker;
    }
}