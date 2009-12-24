using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;
using Game.Logic;
using Game.Util;
using Game.Data.Stats;

namespace Game.Data {
  
    public class TroopObject : GameObject, IPersistableObject {
        TroopStub troopStub;
        public TroopStub Stub {
            get { return troopStub; }
        }

        TroopStats stats;
        public TroopStats Stats {
            get { return stats; }
        }

        public override uint ObjectID {
            get { return objectID; }
            set { CheckUpdateMode(); objectID = value; }
        }

        public override ushort Type {
            get { return 100; }
        }

        public override byte Lvl {
            get { return 0; }
        }

        #region Constructors
        public TroopObject(TroopStub stub) {
            troopStub = stub;
            
            stats = new TroopStats(Formula.GetTroopRadius(troopStub, null), Formula.GetTroopSpeed(troopStub, null));
            stats.StatsUpdate +=new BaseStats.OnStatsUpdate(stats_StatsUpdate);
        }
        #endregion

        #region Updates
        void stats_StatsUpdate() {
            CheckUpdateMode();
        }

        public override void EndUpdate() {
            if (!updating)
                throw new Exception("Called an endupdate without first calling a beginupdate");

            updating = false;

            Update();
        }

        protected new void Update() {            
            base.Update();

            if (!Global.FireEvents) 
                return;

            if (updating)
                return;

            if (objectID > 0)
                Global.dbManager.Save(this);           
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
