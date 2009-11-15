using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Database;
using Game.Util;

namespace Game.Logic {
    public class ReferenceStub : IPersistableObject {
        ushort referenceId;
        public ushort ReferenceId {
            get { return referenceId; }
        }

        ICanDo workerObject;
        public ICanDo WorkerObject {
            get { return workerObject; }
        }

        Action action;
        public Action Action {
            get { return action; }
        }

        public ReferenceStub(ushort id, ICanDo obj, Action action) {
            this.referenceId = id;
            this.workerObject = obj;
            this.action = action;
        }

        #region IPersistable Members
        public const string DB_TABLE = "reference_stubs";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("object_id", workerObject.WorkerId, System.Data.DbType.UInt32),
                    new DbColumn("action_id", action.ActionId, System.Data.DbType.UInt16),
                    new DbColumn("is_active", action is ActiveAction ? true : false, System.Data.DbType.Boolean)
                };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("id", referenceId, System.Data.DbType.UInt16),
                    new DbColumn("city_id", workerObject.City.CityId, System.Data.DbType.UInt32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { };
            }
        }

        bool dbPersisted = false;
        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }
        #endregion
    }


    public class ReferenceManager {
        List<ReferenceStub> reference = new List<ReferenceStub>();
        LargeIdGenerator referenceIdGen = new LargeIdGenerator(ushort.MaxValue);

        ActionWorker actionWorker;

        public ReferenceManager(ActionWorker actionWorker) {
            this.actionWorker = actionWorker;
        }

        public void dbLoaderAdd(ReferenceStub referenceObject) {
            referenceIdGen.set(referenceObject.ReferenceId);
            this.reference.Add(referenceObject);
        }

        public void add(GameObject referenceObject, PassiveAction action) {
                PassiveAction working_stub = actionWorker.PassiveActions[action.ActionId];
                if (working_stub == null)
                    throw new Exception("Action not found");

                ReferenceStub reference = new ReferenceStub((ushort)referenceIdGen.getNext(), referenceObject, working_stub);
                this.reference.Add(reference);
                Global.dbManager.Save(reference);            
        }

        public void add(GameObject referenceObject, ActiveAction action) {
                ActiveAction working_stub = actionWorker.ActiveActions[action.ActionId];
                if (working_stub == null)
                    throw new Exception("Action not found");

                ReferenceStub reference = new ReferenceStub((ushort)referenceIdGen.getNext(), referenceObject, working_stub);
                this.reference.Add(reference);
                Global.dbManager.Save(reference);           
        }

        public void remove(GameObject referenceObject, Action action) {
            this.reference.RemoveAll(delegate(ReferenceStub referenceStub) {
                bool ret = (referenceObject == referenceStub.WorkerObject && referenceStub.Action == action);

                if (ret)
                    Global.dbManager.Delete(referenceStub);

                return ret;
            });
        }

        public IEnumerable<ReferenceStub> getReferences(ICanDo worker) {
            return reference.FindAll(delegate(ReferenceStub stub) {
                return stub.WorkerObject.WorkerId == worker.WorkerId;
            });
        }
    }
}
