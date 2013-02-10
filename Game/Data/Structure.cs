#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Util;
using Persistance;

#endregion

namespace Game.Data
{
    public class Structure : GameObject, IStructure
    {
        public const string DB_TABLE = "structures";

        private readonly ITechnologyManager techmanager;

        private StructureProperties properties;

        private StructureStats stats;

        public Structure(StructureStats stats)
        {
            techmanager = new TechnologyManager(EffectLocation.Object, this, 0);

            this.stats = stats;
            properties = new StructureProperties(this);
        }

        #region Indexers

        public object this[string name]
        {
            get
            {
                return properties.Get(name);
            }
            set
            {
                CheckUpdateMode();
                properties.Add(name, value);
            }
        }

        #endregion

        #region Properties

        public ITechnologyManager Technologies
        {
            get
            {
                return techmanager;
            }
        }

        public StructureProperties Properties
        {
            get
            {
                return properties;
            }
            set
            {
                CheckUpdateMode();
                properties = value;
            }
        }

        public override uint ObjectId
        {
            get
            {
                return base.ObjectId;
            }
            set
            {
                CheckUpdateMode();
                techmanager.Id = value;
                base.ObjectId = value;
            }
        }

        #endregion

        public bool IsMainBuilding
        {
            get
            {
                return ObjectId == 1;
            }
        }

        public StructureStats Stats
        {
            get
            {
                return stats;
            }
            set
            {
                if (stats != null)
                {
                    stats.StatsUpdate -= CheckUpdateMode;
                }

                CheckUpdateMode();
                stats = value;
                stats.StatsUpdate += CheckUpdateMode;
            }
        }

        public override ushort Type
        {
            get
            {
                return stats.Base.Type;
            }
        }

        public byte Lvl
        {
            get
            {
                return stats.Base.Lvl;
            }
        }

        public override void EndUpdate()
        {
            if (!updating)
            {
                throw new Exception("Called endupdate without first calling begin update");
            }

            updating = false;
            Update();
        }

        #region IPersistableObject Members

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("x", X, DbType.UInt32), 
                        new DbColumn("y", Y, DbType.Int32),
                        new DbColumn("hp", stats.Hp, DbType.Decimal), 
                        new DbColumn("type", Type, DbType.Int16),
                        new DbColumn("level", Lvl, DbType.Byte), 
                        new DbColumn("labor", stats.Labor, DbType.UInt16),
                        new DbColumn("is_blocked", IsBlocked, DbType.Boolean),
                        new DbColumn("in_world", InWorld, DbType.Boolean),
                        new DbColumn("state", (byte)State.Type, DbType.Boolean),
                        new DbColumn("state_parameters",
                                     XmlSerializer.SerializeList(State.Parameters.ToArray()),
                                     DbType.String)
                };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[]
                {new DbColumn("id", ObjectId, DbType.UInt32), new DbColumn("city_id", City.Id, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new[] {new DbDependency("Properties", true, true), new DbDependency("Technologies", false, true)};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        protected new void Update()
        {
            base.Update();

            if (!Global.FireEvents)
            {
                return;
            }

            if (updating)
            {
                return;
            }

            DbPersistance.Current.Save(this);
        }
    }
}