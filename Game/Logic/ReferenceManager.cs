#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;

#endregion

namespace Game.Logic
{
    public class ReferenceStub : IPersistableObject
    {
        public const string DB_TABLE = "reference_stubs";

        public ReferenceStub(ushort id, ICanDo obj, GameAction action)
        {
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
                               new DbColumn("object_id", WorkerObject.WorkerId, DbType.UInt32), new DbColumn("action_id", Action.ActionId, DbType.UInt32),
                               new DbColumn("is_active", Action is ActiveAction, DbType.Boolean)
                       };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("id", ReferenceId, DbType.UInt16), new DbColumn("city_id", WorkerObject.City.Id, DbType.UInt32)};
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
    /// Allows adding a reference of an action to an object. E.g. For attaching the AttackChainAction to the moving TroopObject
    /// </summary>
    public class ReferenceManager : IEnumerable<ReferenceStub>
    {
        private readonly IActionWorker actionWorker;
        private readonly List<ReferenceStub> reference = new List<ReferenceStub>();
        private readonly LargeIdGenerator referenceIdGen = new LargeIdGenerator(ushort.MaxValue);

        public ReferenceManager(IActionWorker actionWorker)
        {
            this.actionWorker = actionWorker;
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

        public void DbLoaderAdd(ReferenceStub referenceObject)
        {
            referenceIdGen.Set(referenceObject.ReferenceId);
            reference.Add(referenceObject);
        }

        private void SendAddReference(ReferenceStub referenceObject)
        {
            if (Global.FireEvents)
            {
                //send removal
                var packet = new Packet(Command.ReferenceAdd);
                packet.AddUInt32(actionWorker.City.Id);
                packet.AddUInt16(referenceObject.ReferenceId);
                packet.AddUInt32(referenceObject.WorkerObject.WorkerId);
                packet.AddUInt32(referenceObject.Action.ActionId);

                Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
            }
        }

        private void SendRemoveReference(ReferenceStub referenceObject)
        {
            if (Global.FireEvents)
            {
                //send removal
                var packet = new Packet(Command.ReferenceRemove);
                packet.AddUInt32(actionWorker.City.Id);
                packet.AddUInt16(referenceObject.ReferenceId);

                Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
            }
        }

        public void Add(IGameObject referenceObject, PassiveAction action)
        {

            PassiveAction workingStub;
            if (!actionWorker.PassiveActions.TryGetValue(action.ActionId, out workingStub)) return; 

            var newReference = new ReferenceStub((ushort)referenceIdGen.GetNext(), referenceObject, workingStub);
            reference.Add(newReference);
            DbPersistance.Current.Save(newReference);

            SendAddReference(newReference);
        }

        public void Add(IGameObject referenceObject, ActiveAction action)
        {
            ActiveAction workingStub = actionWorker.ActiveActions[action.ActionId];
            if (workingStub == null)
                throw new Exception("Action not found");

            var newReference = new ReferenceStub((ushort)referenceIdGen.GetNext(), referenceObject, workingStub);
            reference.Add(newReference);
            DbPersistance.Current.Save(newReference);

            SendAddReference(newReference);
        }

        public void Remove(IGameObject referenceObject, GameAction action)
        {
            reference.RemoveAll(referenceStub =>
                {
                    bool ret = (referenceObject == referenceStub.WorkerObject && referenceStub.Action == action);

                    if (ret)
                    {
                        DbPersistance.Current.Delete(referenceStub);

                        SendRemoveReference(referenceStub);
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
                        DbPersistance.Current.Delete(referenceStub);

                        SendRemoveReference(referenceStub);
                    }

                    return ret;
                });
        }

        public IEnumerable<ReferenceStub> GetReferences(ICanDo worker)
        {
            return reference.FindAll(stub => stub.WorkerObject.WorkerId == worker.WorkerId);
        }
    }
}