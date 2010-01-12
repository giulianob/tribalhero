#region

using System.Data;
using Game.Database;

#endregion

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

        protected ushort actionCount;

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
                return new[] {
                                 new DbColumn("type", Type, DbType.UInt16), new DbColumn("worker_type", workerType, DbType.Int32)
                                 , new DbColumn("worker_index", workerIndex, DbType.Byte),
                                 new DbColumn("count", actionCount, DbType.UInt16),
                                 new DbColumn("properties", Properties, DbType.String)
                             };
            }
        }

        #endregion
    }
}