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

        public TroopStub Stub { get; set; }

        TroopStats stats = new TroopStats(0, 0);
        public TroopStats Stats {
            get {
                return stats;
            }
            set {
                if (stats != null)
                    Stats.StatsUpdate -= StatsStatsUpdate;
                
                stats = value;
                stats.StatsUpdate += StatsStatsUpdate;
            }
        }

        public override uint ObjectId {
            get { return objectId; }
            set {
                CheckUpdateMode();
                objectId = value;
            }
        }

        public override ushort Type {
            get { return (ushort) Types.TROOP; }
        }

        public override byte Lvl {
            get { return 0; }
        }

        #region Constructors

        public TroopObject(TroopStub stub) {
            Stub = stub;
        }

        #endregion

        #region Updates

        private void StatsStatsUpdate() {
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
                Global.DbManager.Save(this);
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
                                new DbColumn("is_blocked", IsBlocked, DbType.Boolean),
                                new DbColumn("troop_stub_id", Stub != null ? Stub.TroopId : 0, DbType.Byte),
                                new DbColumn("gold", Stats.Loot.Gold, DbType.Int32),
                                new DbColumn("crop", Stats.Loot.Crop, DbType.Int32),
                                new DbColumn("wood", Stats.Loot.Wood, DbType.Int32),
                                new DbColumn("iron", Stats.Loot.Iron, DbType.Int32),
                                new DbColumn("attack_point", Stats.AttackPoint, DbType.Int32),
                                new DbColumn("attack_radius", Stats.AttackRadius, DbType.Byte),
                                new DbColumn("speed", Stats.Speed, DbType.Byte),
                                new DbColumn("x", X, DbType.UInt32), 
                                new DbColumn("y", Y, DbType.Int32),
                                new DbColumn("in_world", InWorld, DbType.Boolean),
                                new DbColumn("state", (byte) State.Type, DbType.Boolean),
                                new DbColumn("state_parameters", XMLSerializer.SerializeList(State.Parameters.ToArray()), DbType.String)
                             };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new[] {
                                 new DbColumn("id", ObjectId, DbType.UInt32), 
                                 new DbColumn("city_id", City.Id, DbType.UInt32)                                 
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