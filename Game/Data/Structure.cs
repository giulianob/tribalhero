#region

using System;
using System.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Util;

#endregion

namespace Game.Data {
    public class Structure : GameObject, IPersistableObject {
        private StructureProperties properties;
        private TechnologyManager techmanager;

        private StructureStats stats;

        public StructureStats Stats {
            get { return stats; }
            set {
                if (stats != null)
                    stats.StatsUpdate -= new BaseStats.OnStatsUpdate(CheckUpdateMode);

                CheckUpdateMode();
                stats = value;
                stats.StatsUpdate += new BaseStats.OnStatsUpdate(CheckUpdateMode);
            }
        }

        public override ushort Type {
            get { return stats.Base.Type; }
        }

        public override byte Lvl {
            get { return stats.Base.Lvl; }
        }

        public override void EndUpdate() {
            if (!updating)
                throw new Exception("Called endupdate without first calling begin update");

            updating = false;
            Update();
        }

        protected new void Update() {
            base.Update();

            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            Global.dbManager.Save(this);
        }

        public Structure(StructureStats stats) {
            techmanager = new TechnologyManager(EffectLocation.Object, this, 0);

            this.stats = stats;
            properties = new StructureProperties(this);
        }

        #region Indexers

        public object this[string name] {
            get { return properties.get(name); }
            set {
                CheckUpdateMode();
                properties.add(name, value);
            }
        }

        #endregion

        #region Properties

        public TechnologyManager Technologies {
            get { return techmanager; }
        }

        public StructureProperties Properties {
            get { return properties; }
            set {
                CheckUpdateMode();
                properties = value;
            }
        }

        public override uint ObjectId {
            get { return base.ObjectId; }
            set {
                CheckUpdateMode();
                techmanager.ID = value;
                base.ObjectId = value;
            }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "structures";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                                          new DbColumn("x", X, DbType.UInt32), new DbColumn("y", Y, DbType.Int32),
                                          new DbColumn("hp", stats.Hp, DbType.UInt16), new DbColumn("type", Type, DbType.Int16),
                                          new DbColumn("level", Lvl, DbType.Byte), new DbColumn("labor", stats.Labor, DbType.Byte),
                                          new DbColumn("state", (byte) State.Type, DbType.Boolean),
                                          new DbColumn("state_parameters", XMLSerializer.SerializeList(State.Parameters.ToArray()),
                                                       DbType.String)
                                      };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                                          new DbColumn("id", ObjectId, DbType.UInt32), new DbColumn("city_id", city.CityId, DbType.UInt32)
                                      };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[]
                       {new DbDependency("Properties", true, true), new DbDependency("Technologies", false, true)};
            }
        }

        private bool dbPersisted = false;

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        #endregion
    }
}