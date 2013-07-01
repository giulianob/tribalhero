#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Events;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Logic
{
    public class ReferenceStub : IPersistableObject
    {
        private readonly uint cityId;

        public const string DB_TABLE = "reference_stubs";

        public ReferenceStub(ushort id, ICanDo obj, GameAction action, uint cityId)
        {
            this.cityId = cityId;
            ReferenceId = id;
            WorkerObject = obj;
            Action = action;
            
        }

        public ushort ReferenceId { get; private set; }

        public ICanDo WorkerObject { get; private set; }

        public GameAction Action { get; private set; }

        #region IPersistableObject Members

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("object_id", WorkerObject.WorkerId, DbType.UInt32),
                        new DbColumn("action_id", Action.ActionId, DbType.UInt32),
                        new DbColumn("is_active", Action is ActiveAction, DbType.Boolean)
                };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[]
                {new DbColumn("id", ReferenceId, DbType.UInt16), new DbColumn("city_id", cityId, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }

    /// <summary>
    ///     Allows adding a reference of an action to an object. E.g. For attaching the AttackChainAction to the moving TroopObject
    /// </summary>
    public class ReferenceManager : IReferenceManager
    {
        public event EventHandler<ActionReferenceArgs> ReferenceAdded = (sender, args) => { };

        public event EventHandler<ActionReferenceArgs> ReferenceRemoved = (sender, args) => { };

        private readonly uint cityId;

        private readonly IActionWorker actionWorker;

        private readonly ILockable lockingObj;

        private readonly IDbManager dbManager;

        private readonly ILocker locker;

        private readonly List<ReferenceStub> reference = new List<ReferenceStub>();

        private readonly LargeIdGenerator referenceIdGen = new LargeIdGenerator(ushort.MaxValue);

        public ReferenceManager(uint cityId, IActionWorker actionWorker, ILockable lockingObj, IDbManager dbManager, ILocker locker)
        {
            this.cityId = cityId;
            this.actionWorker = actionWorker;
            this.lockingObj = lockingObj;
            this.dbManager = dbManager;
            this.locker = locker;
            
            actionWorker.ActionsRemovedFromWorker += WorkerOnActionsRemovedFromWorker;
            actionWorker.ActionRemoved += WorkerOnActionRemoved;
        }

        public ushort Count
        {
            get
            {
                return (ushort)reference.Count;
            }
        }

        #region IEnumerable<ReferenceStub> Members

        public IEnumerator<ReferenceStub> GetEnumerator()
        {
            return reference.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return reference.GetEnumerator();
        }

        #endregion

        private void WorkerOnActionRemoved(GameAction stub, ActionState state)
        {
            Remove(stub);
        }

        public void DbLoaderAdd(ReferenceStub referenceObject)
        {
            referenceIdGen.Set(referenceObject.ReferenceId);
            reference.Add(referenceObject);
        }

        public void Add(IGameObject referenceObject, PassiveAction action)
        {            
            PassiveAction workingStub;
            if (!actionWorker.PassiveActions.TryGetValue(action.ActionId, out workingStub))
            {
                throw new Exception("Action not found");
            }

            var newReference = new ReferenceStub((ushort)referenceIdGen.GetNext(), referenceObject, workingStub, cityId);
            reference.Add(newReference);
            dbManager.Save(newReference);

            ReferenceAdded(this, new ActionReferenceArgs { ReferenceStub = newReference });
        }

        public void Add(IGameObject referenceObject, ActiveAction action)
        {
            ActiveAction workingStub;
            if (!actionWorker.ActiveActions.TryGetValue(action.ActionId, out workingStub))
            {
                throw new Exception("Action not found");
            }

            var newReference = new ReferenceStub((ushort)referenceIdGen.GetNext(), referenceObject, workingStub, cityId);
            reference.Add(newReference);
            dbManager.Save(newReference);

            ReferenceAdded(this, new ActionReferenceArgs { ReferenceStub = newReference });
        }

        public void Remove(IGameObject referenceObject, GameAction action)
        {
            reference.RemoveAll(referenceStub =>
                {
                    bool ret = (referenceObject == referenceStub.WorkerObject && referenceStub.Action == action);

                    if (ret)
                    {
                        referenceIdGen.Release(referenceStub.ReferenceId);

                        dbManager.Delete(referenceStub);

                        ReferenceRemoved(this, new ActionReferenceArgs { ReferenceStub = referenceStub });
                    }

                    return ret;
                });
        }

        public void Remove(IGameObject referenceObject)
        {
            reference.RemoveAll(referenceStub =>
                {
                    bool ret = (referenceObject == referenceStub.WorkerObject);

                    if (ret)
                    {
                        referenceIdGen.Release(referenceStub.ReferenceId);

                        dbManager.Delete(referenceStub);

                        ReferenceRemoved(this, new ActionReferenceArgs { ReferenceStub = referenceStub });
                    }

                    return ret;
                });
        }

        private void Remove(GameAction action)
        {
            reference.RemoveAll(referenceStub =>
                {
                    bool ret = (action == referenceStub.Action);

                    if (ret)
                    {
                        referenceIdGen.Release(referenceStub.ReferenceId);

                        dbManager.Delete(referenceStub);

                        ReferenceRemoved(this, new ActionReferenceArgs { ReferenceStub = referenceStub });
                    }

                    return ret;
                });
        }

        public IEnumerable<ReferenceStub> GetReferences(ICanDo worker)
        {
            return reference.FindAll(stub => stub.WorkerObject.WorkerId == worker.WorkerId);
        }

        private void WorkerOnActionsRemovedFromWorker(IGameObject workerObject)
        {
            using (locker.Lock(lockingObj))
            {
                Remove(workerObject);
            }
        }
    }
}