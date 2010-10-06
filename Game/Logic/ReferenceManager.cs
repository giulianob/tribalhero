#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game.Logic {
    public class ReferenceStub : IPersistableObject {
        public ushort ReferenceId { get; private set; }

        public ICanDo WorkerObject { get; private set; }

        public GameAction Action { get; private set; }

        public ReferenceStub(ushort id, ICanDo obj, GameAction action) {
            ReferenceId = id;
            WorkerObject = obj;
            Action = action;
        }

        #region IPersistable Members

        public const string DB_TABLE = "reference_stubs";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {
                return new[] {
                                  new DbColumn("object_id", WorkerObject.WorkerId, DbType.UInt32),
                                  new DbColumn("action_id", Action.ActionId, DbType.UInt32),
                                  new DbColumn("is_active", Action is ActiveAction ? true : false, DbType.Boolean)
                              };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new[] {
                                  new DbColumn("id", ReferenceId, DbType.UInt16),
                                  new DbColumn("city_id", WorkerObject.City.Id, DbType.UInt32)
                              };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }

    public class ReferenceManager : IEnumerable<ReferenceStub> {
        private readonly List<ReferenceStub> reference = new List<ReferenceStub>();
        private readonly LargeIdGenerator referenceIdGen = new LargeIdGenerator(ushort.MaxValue);

        private readonly ActionWorker actionWorker;

        public ushort Count {
            get { return (ushort)reference.Count; }
        }

        public ReferenceManager(ActionWorker actionWorker) {
            this.actionWorker = actionWorker;
        }

        public void DbLoaderAdd(ReferenceStub referenceObject) {
            referenceIdGen.Set(referenceObject.ReferenceId);
            reference.Add(referenceObject);
        }

        private void SendAddReference(ReferenceStub referenceObject) {

            if (Global.FireEvents)
            {
                //send removal
                Packet packet = new Packet(Command.REFERENCE_ADD);
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
                Packet packet = new Packet(Command.REFERENCE_REMOVE);
                packet.AddUInt32(actionWorker.City.Id);
                packet.AddUInt16(referenceObject.ReferenceId);

                Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
            }
        }

        public void Add(GameObject referenceObject, PassiveAction action) {
            PassiveAction workingStub = actionWorker.PassiveActions[action.ActionId];
            if (workingStub == null) {
                throw new Exception("Action not found");
            }

            ReferenceStub newReference = new ReferenceStub((ushort) referenceIdGen.GetNext(), referenceObject, workingStub);
            reference.Add(newReference);
            Global.DbManager.Save(newReference);

            SendAddReference(newReference);
        }    

        public void Add(GameObject referenceObject, ActiveAction action) {
            ActiveAction workingStub = actionWorker.ActiveActions[action.ActionId];
            if (workingStub == null)
                throw new Exception("Action not found");

            ReferenceStub newReference = new ReferenceStub((ushort) referenceIdGen.GetNext(), referenceObject, workingStub);
            reference.Add(newReference);
            Global.DbManager.Save(newReference);

            SendAddReference(newReference);
        }

        public void Remove(GameObject referenceObject, GameAction action) {
            reference.RemoveAll(referenceStub => {
                bool ret = (referenceObject == referenceStub.WorkerObject && referenceStub.Action == action);

                if (ret) {
                    Global.DbManager.Delete(referenceStub);

                    SendRemoveReference(referenceStub);
                }

                return ret;
            });
        }

        public void Remove(GameObject referenceObject) {
            reference.RemoveAll(referenceStub => {
                bool ret = (referenceObject == referenceStub.WorkerObject);

                if (ret) {
                    Global.DbManager.Delete(referenceStub);
                    
                    SendRemoveReference(referenceStub);
                }

                return ret;
            });
        }

        public IEnumerable<ReferenceStub> GetReferences(ICanDo worker) {
            return reference.FindAll(stub => stub.WorkerObject.WorkerId == worker.WorkerId);
        }

        #region IEnumerable<ReferenceStub> Members

        public IEnumerator<ReferenceStub> GetEnumerator()
        {
            return reference.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return reference.GetEnumerator();
        }

        #endregion
    }
}