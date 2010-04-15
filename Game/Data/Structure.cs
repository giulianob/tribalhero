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
        private readonly TechnologyManager techmanager;

        private StructureStats stats;

        public StructureStats Stats {
            get { return stats; }
            set {
                if (stats != null)
                    stats.StatsUpdate -= CheckUpdateMode;

                CheckUpdateMode();
                stats = value;
                stats.StatsUpdate += CheckUpdateMode;
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

            Global.DbManager.Save(this);
        }

        public Structure(StructureStats stats) {
            techmanager = new TechnologyManager(EffectLocation.OBJECT, this, 0);

            this.stats = stats;
            properties = new StructureProperties(this);
        }

        #region Indexers

        public object this[string name] {
            get { return properties.Get(name); }
            set {
                CheckUpdateMode();
                properties.Add(name, value);
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
                techmanager.Id = value;
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
                return new[] {
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
                return new[] {
                                          new DbColumn("id", ObjectId, DbType.UInt32), new DbColumn("city_id", city.Id, DbType.UInt32)
                                      };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new[]
                       {new DbDependency("Properties", true, true), new DbDependency("Technologies", false, true)};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}