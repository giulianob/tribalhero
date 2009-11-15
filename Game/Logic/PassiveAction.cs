using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;

namespace Game.Logic {
    public abstract class PassiveAction: Action {

        public PassiveAction() { }

        public PassiveAction(ushort id, bool isVisible) {
            this.ActionId = id;
            this.IsVisible = isVisible;
        }

        bool isVisible = false;
        public bool IsVisible {
            get { return isVisible; }
            set { isVisible = value; }
        }

        public const string DB_TABLE = "passive_actions";

        public override string DbTable {
            get { return DB_TABLE; }
        }

        bool isChain = false;
        public bool IsChain {
            get { return isChain; }
            set { isChain = value; }
        }

        public override DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("is_chain", isChain, System.Data.DbType.Boolean),
                    new DbColumn("is_scheduled", false, System.Data.DbType.Boolean),
                    new DbColumn("is_visible", isVisible, System.Data.DbType.Boolean),
                    new DbColumn("type", Type, System.Data.DbType.UInt32),
                    new DbColumn("properties", Properties, System.Data.DbType.String),                   
                };
            }
        }
    }
}