#region

using System.Data;
using Persistance;

#endregion

namespace Game.Logic
{
    public abstract class ActiveAction : GameAction
    {
        public const string DB_TABLE = "active_actions";

        public int WorkerType { get; set; }

        public byte WorkerIndex { get; set; }

        public ushort ActionCount { get; protected set; }

        public abstract ConcurrencyType ActionConcurrency { get; }

        public override string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public override DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("object_id", WorkerObject.WorkerId, DbType.UInt32),
                        new DbColumn("type", Type, DbType.UInt16), new DbColumn("worker_type", WorkerType, DbType.Int32)
                        , new DbColumn("worker_index", WorkerIndex, DbType.Byte),
                        new DbColumn("count", ActionCount, DbType.UInt16),
                        new DbColumn("properties", Properties, DbType.String)
                };
            }
        }
    }
}