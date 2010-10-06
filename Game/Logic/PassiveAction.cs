#region

using System.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    public abstract class PassiveAction : GameAction {
        protected PassiveAction() {}

        protected PassiveAction(uint id, bool isVisible) {
            ActionId = id;
            IsVisible = isVisible;
        }

        private bool isVisible;

        public bool IsVisible {
            get { return isVisible; }
            set { isVisible = value; }
        }

        public const string DB_TABLE = "passive_actions";

        public override string DbTable {
            get { return DB_TABLE; }
        }

        private bool isChain;

        public bool IsChain {
            get { return isChain; }
            set { isChain = value; }
        }

        public bool isCancellable { get; protected set; }

        public override DbColumn[] DbColumns {
            get {
                return new[] {
                                new DbColumn("object_id", WorkerObject.WorkerId, DbType.UInt32),
                                new DbColumn("is_chain", isChain, DbType.Boolean),
                                new DbColumn("is_scheduled", false, DbType.Boolean),
                                new DbColumn("is_visible", isVisible, DbType.Boolean), 
                                new DbColumn("type", Type, DbType.UInt32), 
                                new DbColumn("properties", Properties, DbType.String),
                            };
            }
        }
    }
}