using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Database;

namespace Game.Logic {

    public abstract class ActiveAction : Action {       

        protected int workerType;
        public int WorkerType {
            get { return workerType; }
            set { workerType = value; }
        }

        protected byte workerIndex;
        public byte WorkerIndex {
            get { return workerIndex; }
            set { workerIndex = value; }
        }
        
        protected ushort actionCount = 0;
        public ushort ActionCount {
            get { return actionCount; }
        }

        #region IPersistable Members
        public const string DB_TABLE = "active_actions";

        public override string DbTable {
            get { return DB_TABLE; }
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {                    
                    new DbColumn("type", Type, System.Data.DbType.UInt16),
                    new DbColumn("worker_type", workerType, System.Data.DbType.Int32),
                    new DbColumn("worker_index", workerIndex, System.Data.DbType.Byte),
                    new DbColumn("count", actionCount, System.Data.DbType.UInt16),
                    new DbColumn("properties", Properties, System.Data.DbType.String)
                };
            }
        }
        #endregion
    }

}
