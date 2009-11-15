using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;
using Game.Logic;
using Game.Util;

namespace Game.Data {
  
    public class TroopObject : GameObject, IPersistableObject {
        TroopStub troopStub;
        public TroopStub Stub {
            get { return troopStub; }
        }

        int rewardPoint;
        public int RewardPoint {
            get { return rewardPoint; }
            set { rewardPoint = value; }
        }

        Resource loot = new Resource();
        public Resource Loot {
            get { return loot; }
            set { loot = value; }
        }

        TroopStats stats;
        public TroopStats Stats {
            get { return stats; }
            set { stats = value; }
        }

        public override uint ObjectID {
            get { return objectID; }
            set { 
                objectID = value;
            }
        }

        #region Constructors
        private TroopObject() {
        }

        public TroopObject(TroopStub stub) {
            Type = 100;
            troopStub = stub;            
            UpdateStatus();
        }
        #endregion

        #region Members
        private void UpdateStatus() {
            this.Stats = new TroopStats((byte)Formula.GetTroopRadius(troopStub,null), Formula.GetTroopSpeed(troopStub,null));
        }
        #endregion

        #region Events

        public override void EndUpdate() {
            updating = false;

            Update();
        }

        public new void Update() {            
            base.Update();

            if (!Global.FireEvents) 
                return;

            if (updating)
                return;

            if (objectID > 0)
                Global.dbManager.Save(this);

            if (troopStub != null)
                troopStub.FireUpdated();            
        }
        #endregion

        #region IPersistable Members
        public const string DB_TABLE = "troops";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {                    
                    new DbColumn("x", X, System.Data.DbType.UInt32),
                    new DbColumn("y", Y, System.Data.DbType.Int32),
                    new DbColumn("state", (byte)State.Type, System.Data.DbType.Boolean),
                    new DbColumn("state_parameters", XMLSerializer.SerializeList(State.Parameters.ToArray()), System.Data.DbType.String)
                };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("id", ObjectID, System.Data.DbType.UInt32),
                    new DbColumn("city_id", City.CityId, System.Data.DbType.UInt32),
                    new DbColumn("troop_stub_id", Stub.TroopId, System.Data.DbType.Byte)
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
}
