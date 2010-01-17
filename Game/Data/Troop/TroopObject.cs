#region

using System;
using System.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Data {
    public class TroopObject : GameObject, IPersistableObject {
        private TroopStub troopStub;

        public TroopStub Stub {
            get { return troopStub; }
        }

        private TroopStats stats;

        public TroopStats Stats {
            get { return stats; }
        }

        public override uint ObjectId {
            get { return objectId; }
            set {
                CheckUpdateMode();
                objectId = value;
            }
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
            stats.StatsUpdate += stats_StatsUpdate;
        }

        #endregion

        #region Updates

        private void stats_StatsUpdate() {
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

            if (objectId > 0)
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
                return new[] {
                                 new DbColumn("x", X, DbType.UInt32), new DbColumn("y", Y, DbType.Int32),
                                 new DbColumn("state", (byte) State.Type, DbType.Boolean),
                                 new DbColumn("state_parameters", XMLSerializer.SerializeList(State.Parameters.ToArray()),
                                              DbType.String)
                             };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new[] {
                                 new DbColumn("id", ObjectId, DbType.UInt32), new DbColumn("city_id", City.Id, DbType.UInt32)
                                 , new DbColumn("troop_stub_id", Stub.TroopId, DbType.Byte)
                             };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}